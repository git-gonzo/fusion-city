<?php
ini_set ( 'display_errors', 1 );
error_reporting(E_ALL);
//Connect To Database
$hostname='localhost';
$username='newmates_h4lqIbOi';
$password='p16rAPfVo2u7cCeX';
$dbname='newmates_h4lqIbOi';

$conn = mysqli_connect($hostname,$username, $password, $dbname) OR DIE ('Unable to connect to database! Please try again later.');

	$playerID = $_GET["playerid"];

	$playerexists = 0;
	$pendingNotification = false;

if(empty($playerID)){
	echo "NO PLAYER ID";
}
else
{
	$query = "SELECT * FROM MLF_PlayerState WHERE playerID = " . $playerID;

	if($resultState = $conn->query($query)){
		$playerexists = 1;
	}

	if(!$playerexists){
		
		$playername = $_GET["playername"];
		$charindex = $_GET["charindex"];
		$level = $_GET["level"];
		$xp = $_GET["gold"];
		$xp = $_GET["fame"];
		$gems = $_GET["gems"];
		$activities = $_GET["activities"];
		$jobs = $_GET["jobs"];
		$travels = $_GET["travels"];
		$location = $_GET["location"];

		if(empty($playerID) || empty($charindex) || empty($playername)  ){
			echo "Error: missing data";
		}
		else 
		{
			if(empty($level)) $level = 1;
			if(empty($xp)) $xp = 0;
			if(empty($gold)) $gold = 0;
			if(empty($gems)) $gems = 0;
			if(empty($fame)) $fame = 0;
			if(empty($activities)) $activities = 0;
			if(empty($jobs)) $jobs = 0; 
			if(empty($travels)) $travels = 0;
			if(empty($location)) $location = 1;
			//******* Insert new player and return ID
			$query = "INSERT INTO MLF_PlayerState (playername,characterIndex,level,xp,fame,gold,gems,activities,jobs,travels,location) VALUES ('".$playername."',".$charindex.",".$level.",".$xp.",".$fame.",".$gold.",".$gems.",".$activities.",".$jobs.",".$travels.",".$location.")";
			if ( $conn->query($query)) {
		 	    echo "Success Player Added to PlayerState";
			} else {
		    	echo "Error";
			}
		}
	}
	else
	{
		//Check Pending notifications
		$query = "SELECT * FROM MLF_PlayerNotification WHERE playerID = " . $playerID . " AND Notified = 0 LIMIT 1" ;
		if($resultNotif = $conn->query($query)) {
			$rowState = $resultState->fetch_object(); 
			$rowNotif = $resultNotif->fetch_object();
			//echo $rowNotif->newFame; 
			if($rowNotif->newFame > 0) {$fame = $rowNotif->newFame;} else {$fame = $rowState->fame;}
			if($rowNotif->newGold > 0) {$gold = $rowNotif->newGold;} else {$gold = $rowState->gold;}
			if($rowNotif->newGems > 0) {$gems = $rowNotif->newGems;} else {$gems = $rowState->gems;}

			$query = "UPDATE MLF_PlayerState SET fame = ".$fame.", gold = ".$gold.", gems = ".$gems." WHERE playerID = " . $playerID ;
			$conn->query($query);
			$query = "UPDATE MLF_PlayerNotification SET Notified = 1, timespam = CURRENT_TIMESTAMP WHERE playerID = " . $playerID ;
			$conn->query($query);
		}
		else
		{
			echo "No Notifications for player " . $playerID;
		}
	}
}

?>