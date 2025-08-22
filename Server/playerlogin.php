<?php
//ini_set ( 'display_errors', 1 );
//error_reporting(E_ALL);
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
	$array = array();
	$query = "SELECT * FROM MLF_PlayerState WHERE playerID = ? LIMIT 1";
	$statement = $conn->prepare($query);
	$statement->bind_param("i",$playerID);
	$statement->execute();
	$resultState = $statement->get_result();
	if($resultState->num_rows == 1){
		$playerexists = 1;
	}

	$playername = $_GET["playername"];
	$level = $_GET["level"];
	$xp = $_GET["xp"];
	$gold = $_GET["gold"];
	$fame = $_GET["fame"];
	$gems = $_GET["gems"];
	$activities = $_GET["activities"];
	$jobs = $_GET["jobs"];
	$travels = $_GET["travels"];

	if(!$playerexists){
		$charindex = $_GET["charindex"];
		$location = $_GET["location"];

		if(empty($playername)  ){
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
		 	    //Return PlayerID
		 		$query = "SELECT playerID FROM MLF_PlayerState WHERE playername = '" .$playername. "' ORDER BY PlayerID DESC LIMIT 1";
		 		$RestulPlayerID = $conn->query($query);
		 		$row = $RestulPlayerID->fetch_object(); 
		 		$playerID = $row->playerID;
		 		$toAdd = array("result"=>"NewPlayer","playerID"=>$playerID);
			} else {
				$toAdd = array("result"=>"Error","descrip"=>"Error in query inset into player state");
			}
		}
	}
	else
	{
		//Update Player State
		$query = "UPDATE MLF_PlayerState SET level = ".$level.", xp = ".$xp.", fame = ".$fame.", gold = ".$gold.", gems = ".$gems.", activities = ".$activities.", jobs = ".$jobs.", travels = ".$travels." WHERE playerID = " . $playerID ;
		$conn->query($query);

		//Check Pending notifications
		$query = "SELECT * FROM MLF_PlayerNotification WHERE playerID = " . $playerID . " AND Notified = 0 LIMIT 1" ;
		$resultNotif = $conn->query($query);
		if($resultNotif->num_rows) {
			$rowState = $resultState->fetch_object(); 
			$rowNotif = $resultNotif->fetch_object();
			$fameUpdated = 0;
			$goldUpdated = 0;  
			$gemsUpdated = 0;
			$levelUpdated = 0;
			$updates = 0;
			//echo $rowNotif->newFame; 
			if($rowNotif->newFame > 0) {$fameUpdated = 1; $fame = $rowNotif->newFame;} else {$fame = $rowState->fame;}
			if($rowNotif->newGold > 0) {$goldUpdated = 1; $gold = $rowNotif->newGold;} else {$gold = $rowState->gold;}
			if($rowNotif->newGems > 0) {$gemsUpdated = 1; $gems = $rowNotif->newGems;} else {$gems = $rowState->gems;}
			if($rowNotif->newLevel > 0) {$levelUpdated = 1; $level = $rowNotif->newLevel;} else {$level = $rowState->level;}

			$query = "UPDATE MLF_PlayerState SET fame = ".$fame;
			if($goldUpdated) $query = $query .", gold = ".$gold;
			if($gemsUpdated) $query = $query .", gems = ".$gems;
			if($levelUpdated) $query = $query .", level = ".$level;
			$query = $query." WHERE playerID = " . $playerID ;
			$conn->query($query);
			$query = "UPDATE MLF_PlayerNotification SET Notified = 1, timestamp = CURRENT_TIMESTAMP WHERE playerID = " . $playerID ;
			$conn->query($query);
			$toAdd = array("result"=>"Notification","goldUpdated"=>$goldUpdated,"newGold"=>$gold,"gemsUpdated"=>$gemsUpdated,"newGems"=>$gems,"fameUpdated"=>$fameUpdated,"newFame"=>$fame,"levelUpdated"=>$levelUpdated,"newLevel"=>$level,"message"=>$rowNotif->message);
		}
		else
		{
			//NO NOTIFICATIONS
			$toAdd = array("result"=>"PlayerExists");
		}
	}

	array_push($array ,$toAdd);
	$json = json_encode($array, JSON_UNESCAPED_UNICODE);
	echo $json;
	$query = "INSERT INTO MLF_PlayerLogin (playerID,playername,timestamp) VALUES (".$playerID.",'".$playername."',CURRENT_TIMESTAMP)";
	$conn->query($query);
}

?>