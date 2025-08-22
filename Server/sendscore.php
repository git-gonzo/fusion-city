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
	$score = $_GET["score"];
	$level = $_GET["level"];
	$xp = $_GET["xp"];

	$query = "SELECT * FROM MLF_Leaderboard where player = ?";
	$statement = $conn->prepare($query);
	$statement->bind_param("s",$player);
	$statement->execute();
	$result = $statement->get_result();

if($result->num_rows == 0){
	echo "No existe";
	$query = "INSERT INTO MLF_Leaderboard (player, score, level, xp) VALUES ('".$player."',".$score.",".$level.",".$xp.")";
	if ( $conn->query($query)) {
 	    echo "Success";
	} else {
    	echo "Error";
	}
}
else
{
	$query = "UPDATE MLF_Leaderboard SET score = ".$score.", level = ".$level.", xp = ".$xp." WHERE player = '".$player."'";
	if ( $conn->query($query)) {
 	   echo "Success";
	} else {
    	echo "Error";
	}
}
?>