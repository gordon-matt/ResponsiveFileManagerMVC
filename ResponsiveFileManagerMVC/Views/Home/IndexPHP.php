<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>PHP View Engine Demo</title>
    <link href="/Content/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="/Content/Site.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <div class="navbar navbar-inverse navbar-fixed-top">
        <div class="container">
            <div class="navbar-header">
                <button type="button" class="navbar-toggle" data-toggle="collapse" data-target=".navbar-collapse">
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                </button>
                <a href="/" class="navbar-brand">PHP MVC</a>
            </div>
            <div class="navbar-collapse collapse">
                <ul class="nav navbar-nav">
                    <li><a href="/" class="navbar-brand">Home</a></li>
                    <li><a href="/Home/About" class="navbar-brand">About</a></li>
                    <li><a href="/Home/FileManager" class="navbar-brand">File Manager</a></li>
                </ul>
            </div>
        </div>
    </div>

    <div class="container body-content">
        <p>Stan says hello from static HTML.<p>
        <?$arr = array(1 => "Dave", 2 => "Penny", 3 => "Tim");?>
        <p><? echo $arr[2]." says hello from PHP!"; ?></p>
        <? echo $viewdata["message"]; ?>
        <hr />
        <p>Do question marks screw things up?</p>
        <p>
            <?
	            $a = $viewdata['a'];
	            $b = $viewdata['b'];
	
	            echo "$a + $b = ".adder($a,$b);
            ?>
        </p>
        <ol>
            <?
                foreach ($viewdata['numbers'] as $number)
                {
	                echo "<li>$number</li>";
                }
            ?>
        </ol>
        <ul>
            <?
                // I love the PHP foreach syntax here
                foreach ($model as $name => $age)
                {
	                echo "<li>$name is $age</li>";
                }
            ?>
        </ul>
        <table class="table table-striped">
            <?
                foreach($route as $key => $value)
                {
	                echo "<tr><td>$key</td><td>$value</td></tr>";
                }
            ?>
        </table>
        <hr />
        Enter a person's name: 
        <? $url = '/'.$route['controller'].'/Display'; ?>
        <form action='<? echo $url; ?>' enctype='multipart/form-data' method="post">
            <input type='text' class="form-control" name='freetext' />
            <input type='submit' class="btn btn-primary" value='Submit' />
        </form>
        <?
            /* I put the function way down here
             * to demonstrate that it does not
             * have to come before first use.
             */
            function adder ($x, $y)
            {
                return $x+$y;
            }
        ?>
        <hr />
        <footer>
            <p>&copy; @DateTime.Now.Year - My ASP.NET Application</p>
        </footer>
    </div>

    <script src="/Scripts/jquery-2.1.4.min.js" type="text/javascript"></script>
    <script src="/Scripts/bootstrap.min.js" type="text/javascript"></script>
</body>
</html>