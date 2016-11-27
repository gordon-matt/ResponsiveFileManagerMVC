using System;
using System.Collections;
using System.IO;
using System.Web.Mvc;
using PHP.Core;
using PHP.Core.Reflection;

namespace ResponsiveFileManagerMVC.ViewEngine
{
    public class PhpView : IView
    {
        public PhpView(string viewPath, string masterPath)
        {
            this.ViewPath = viewPath;
            this.MasterPath = masterPath;
        }

        public string ViewPath { get; private set; }

        public string MasterPath { get; private set; }

        // This method reads in the view, processes it, and outputs the resulting HTML
        public void Render(ViewContext context, TextWriter writer)
        {
            // Find the view and read the contents into a string
            string filename = context.HttpContext.Server.MapPath(this.ViewPath);
            string contents = File.ReadAllText(filename);

            // Create the PHP context and wire output to TextWriter
            // We cannot use CurrentContext or function names may clash
            var sc = new ScriptContext(ApplicationContext.Default);
            sc.Output = writer;

            // Copy routedata into PHP context as a PHP array
            var ra = new PhpArray();
            var routedata = context.RouteData;
            foreach (var v in routedata.Values)
            {
                ra.SetArrayItem(v.Key, v.Value);
            }
            Operators.SetVariable(sc, null, "route", ra);

            // Copy viewdata into PHP context as a PHP array
            var pa = new PhpArray();
            var viewdata = context.ViewData;
            foreach (var item in viewdata)
            {
                var val = PhpSafeType(item.Value);
                pa.SetArrayItem(item.Key, val);
            }
            Operators.SetVariable(sc, null, "viewdata", pa);

            // Copy the model into the PHP context
            if (viewdata.Model != null)
            {
                Operators.SetVariable(sc, null, "model", PhpSafeType(viewdata.Model));
            }

            // Parse the PHP/HTML and process it
            PhpEval(sc, Parse(contents));
        }

        // Ensure that types are in a form that PHP can handle
        // eg. floats to doubles and collections to PHP arrays
        // TODO: Handle complex objects other than collections
        private object PhpSafeType(object o)
        {
            // PHP can handle bool, int, double, and long
            if ((o is int) || (o is double) || (o is long) || (o is bool))
            {
                return o;
            }
            // Upcast other integer types so PHP can use them
            // TODO: What to do about System.UInt64 and byte?
            else if (o is short)
            {
                return (int)(short)o;
            }
            else if (o is ushort)
            {
                return (int)(ushort)o;
            }
            else if (o is uint)
            {
                return (long)(uint)o;
            }
            else if (o is ulong)
            {
                ulong u = (ulong)o;
                if (u <= Int64.MaxValue)
                {
                    return System.Convert.ToInt64(u);
                }
                else
                {
                    return u.ToString();
                }
            }
            // Convert System.Single to a string
            // to reduce rounding errors
            // TODO: Figure out why I need to do this
            else if (o is float)
            {
                return Double.Parse(o.ToString());
            }
            // Really not sure what the best thing is to do with 'System.Decimal'
            // TODO: Review this decision
            else if (o is decimal)
            {
                return o.ToString();
            }
            // Strings and byte arrays require special handling
            else if (o is string)
            {
                return new PhpString((string)o);
            }
            else if (o is byte[])
            {
                return new PhpBytes((byte[])o);
            }
            // Convert .NET collections into PHP arrays
            else if (o is ICollection)
            {
                var ca = new PhpArray();
                if (o is IDictionary)
                {
                    var dict = o as IDictionary;
                    foreach (var key in dict.Keys)
                    {
                        var val = PhpSafeType(dict[key]);
                        ca.SetArrayItem(PhpSafeType(key), val);
                    }
                }
                else
                {
                    foreach (var item in (ICollection)o)
                    {
                        ca.Add(PhpSafeType(item));
                    }
                }
                return ca;
            }
            // PHP types are obviously ok and can just move along
            if (o is DObject)
            {
                return o;
            }
            // Wrap all remaining CLR types so that PHP can handle tham
            return ClrObject.WrapRealObject(o);
        }

        // Send code block to Phalanger to be executed
        // TODO: include line numbers, file names, and other useful stuff
        private object PhpEval(ScriptContext context, string code)
        {
            return DynamicCode.Eval(
                code,
                false,
                context,
                null,
                null,
                null,
                "default",
                1, 1,
                -1,
                null
                );
        }

        // Brute force parser
        // TODO: Allow <? and ?> to be escaped in HTML
        // TODO: Ignore <? and ?> when in single or double quoted strings in code
        // TODO: Handle <?php in addition to <?
        // TODO: Give this some Pratt parser love
        private string Parse(string input)
        {
            var sb = new System.Text.StringBuilder();
            var codeBuilder = new System.Text.StringBuilder();
            int p = 0;
            char c;
            bool incode = false;
            int line = 1;
            int startline = 0;
            while (p < input.Length)
            {
                c = input[p];
                switch (c)
                {
                    case '\n':
                        line++;
                        goto default;
                    case '"':
                        if (incode) goto default;
                        sb.Append("\\\"");	// We need to escape out double quotes for use in 'echo'
                        p++;
                        break;

                    case '<':
                        p++;
                        if (p < input.Length)
                        {
                            if ((input[p] == '?'))
                            {
                                if (incode) throw new Exception("Nested PHP code block at line " + line);
                                incode = true;
                                startline = line;
                                // Convert HTML string into an echo statement
                                codeBuilder.AppendFormat("echo \"{0}\";", sb.ToString());
                                sb = new System.Text.StringBuilder();
                                p++;	// Skip the ?
                            }
                            else
                            {
                                sb.Append('<');
                            }
                        }
                        else
                        {
                            sb.Append('<');
                        }
                        break;

                    case '?':
                        if (incode)
                        {
                            p++;
                            if (p < input.Length)
                            {
                                if (input[p] == '>')
                                {
                                    codeBuilder.AppendLine(sb.ToString());
                                    incode = false;
                                    sb = new System.Text.StringBuilder();
                                    p++; // Skip the >
                                }
                            }
                            else
                            {
                                sb.Append('?');
                            }
                        }
                        else
                        {
                            sb.Append('?');
                            p++;
                        }
                        break;

                    default:
                        sb.Append(c);
                        p++;
                        break;
                }
            }

            // Ensure that all PHP code blocks were properly closed
            if (incode) throw new Exception("Unclosed PHP code block at line " + startline);

            // Flush any remaining HTML
            codeBuilder.AppendFormat("echo \"{0}\";", sb.ToString());

            return codeBuilder.ToString();
        }
    }

    // Small class to hold the parse results
    internal class Token
    {
        public Token(string type, string text)
        {
            Type = type;
            Text = text;
        }

        public string Type { get; set; }

        public string Text { get; set; }
    }
}