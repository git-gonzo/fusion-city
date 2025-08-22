<?php
ini_set ( 'display_errors', 1 );
error_reporting(E_ALL);
//Connect To Database
$hostname='localhost';
$username='newmates_h4lqIbOi';
$password='p16rAPfVo2u7cCeX';
$dbname='newmates_h4lqIbOi';

$conn = mysqli_connect($hostname,$username, $password, $dbname) OR DIE ('Unable to connect to database! Please try again later.');

	$playerID = $_GET["playerID"];
	$action = $_GET["action"];
	$resource = $_GET["resource"];
	$amount = $_GET["amount"];
	$targetID = $_GET["targetID"];

	$query = "INSERT INTO MLF_PlayerAction (playerID, actionID, resourceType, amount, targetID, timestamp) VALUES (".
	$playerID.",".$action.",".$resource.",".$amount.",".$targetID.",CURRENT_TIMESTAMP())";
	if ( $conn->query($query)) {
 	    echo "Success";
	} else {
    	echo "Error";
	}

?>