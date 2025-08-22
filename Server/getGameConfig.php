<?php
ini_set ( 'display_errors', 1 );
error_reporting(E_ALL);
//Connect To Database
$hostname='localhost';
$username='newmates_h4lqIbOi';
$password='p16rAPfVo2u7cCeX';
$dbname='newmates_h4lqIbOi';

$conn = mysqli_connect($hostname,$username, $password, $dbname) OR DIE ('Unable to connect to database! Please try again later.');

$query = "SELECT * FROM MLF_Buildings";
$result = $conn->query($query);
$array = array();

while($row = $result->fetch_object()){
	//echo $row->player;
	$toAdd = array("buildingID"=>$row->BuildingID,"stringKey"=>$row->stringKey,"currency"=>$row->currency,"profit"=>$row->profit,"price"=>$row->price,"owner"=>$row->Owner);
	array_push($array ,$toAdd);
}
    
$result->close();

$jsonresult1 = json_encode($array, JSON_UNESCAPED_UNICODE);
echo $jsonresult1;
?>