<?php
ini_set ( 'display_errors', 1 );
error_reporting(E_ALL);
//Connect To Database
$hostname='localhost';
$username='newmates_h4lqIbOi';
$password='p16rAPfVo2u7cCeX';
$dbname='newmates_h4lqIbOi';
$yourfield = 'Nombre';
$conn = mysqli_connect($hostname,$username, $password, $dbname) OR DIE ('Unable to connect to database! Please try again later.');

	$player = $_GET["playername"];
	$activity = $_GET["activity"];
	$state = $_GET["state"];

	$query = "INSERT INTO MLF_PlayerActivities (player, activity, state, datecompleted) VALUES ('".$player."','".$activity."',".$state.",CURRENT_TIMESTAMP())";
	if ( $conn->query($query)) {
 	    echo "Success";
	} else {
    	echo "Error";
	}

?>