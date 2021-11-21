//This file is part of Tribes RPG.
//Tribes RPG client side scripts
//Repack RPG additions written by Jason "phantom" Daley, tribesrpg.org

//	Copyright (C) 2019  Jason Daley

//	This program is free software: you can redistribute it and/or modify
//	it under the terms of the GNU General Public License as published by
//	the Free Software Foundation, either version 3 of the License, or
//	(at your option) any later version.

//	This program is distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//	GNU General Public License for more details.

//	You should have received a copy of the GNU General Public License
//	along with this program.  If not, see <http://www.gnu.org/licenses/>.

//You may contact the author at beatme101@gmail.com or www.tribesrpg.org/contact.php

//This GPL does not apply to Starsiege: Tribes or any non-RPG related files included.
//Starsiege: Tribes, including the engine, retains a proprietary license forbidding resale.

//________________________________________________________________________________________________________________________________________________________________
// DescX Notes:
//		comchat.cs was such a bloated, upsetting mess that it caused me to become angry several times. I could not continue without cleaning it up.
//		Comchat is now split into separate files for admin, player, and skill functions.
//
//		The basic call to internalsay simply switches through functions related to each group of commands, and I've corrected the grevious error
//		of forgetting to use the "else" keyword. This cleans up the functions massively. Comchat is now readable and is 100% less likely to cause heart attacks :)


$MsgTypeSystem = 0;
$MsgTypeGame = 1;
$MsgTypeChat = 2;
$MsgTypeTeamChat = 3;
$MsgTypeCommand = 4;

$MsgWhite = 0;
$MsgRed = 1;
$MsgBeige = 2;
$MsgGreen = 3;

function remoteSay(%clientId, %team, %message, %senderName)
{
	//tribesrpg.org
	if(%sendername != ""){
		%n = Client::getName(%ClientId);
		%ip = Client::getTransportAddress(%ClientId);
		messageall(0,"Exploit attempt detected and blocked: " @ %ClientId @ ", aka " @ %n @ ", at " @ %ip @ ".");
		messageall(0,"Exploit: " @ %message);
		echo("Exploit attempt detected and blocked: " @ %ClientId @ ", aka " @ %n @ ", at " @ %ip @ ".");
		echo("Exploit: " @ %message);
		return;
	}
	internalSay(%clientId, %team, %message);
}

//________________________________________________________________________________________________________________________________________________________________
// All instances of "remotesay" in all scripts should be changed to "internalsay".
// Old comments in this code STRONGLY indicate that none of the lines above actual command processing should be changed.
// This function is entered in a variety of ways. The cluster of logic at the top looks nonsensical, but it's actually required code.
//________________________________________________________________________________________________________________________________________________________________
function internalSay(%clientId, %team, %message, %senderName)
{
	dbecho($dbechoMode, "internalSay(" @ %clientId @ ", " @ %team @ ", \"" @ %message @ "\", " @ %senderName @ ")");
	
	if(%clientId.IsInvalid)
		return;

	%TrueClientId = %clientId;
	if(%senderName == "nofloodprotect") {
		%clientId.lastSayTime=0;
		%senderName = "";
	}
	
	if(%senderName != "") {
		%clientId = 2048;
		%clientToServerAdminLevel = $BlockOwnerAdminLevel[%senderName];
	} else {
		%senderName = Client::getName(%clientId);
		%clientToServerAdminLevel = floor(%clientId.adminLevel);
	}
	
	%isai = Player::isAiControlled(%clientId);
	if(%isai)
		%clientToServerAdminLevel = 3;

	if(%TrueClientId == 2048)	%echoOff = True;
	else						%echoOff = %TrueClientId.echoOff;

	if(%TrueClientId != 2048)	%TCsenderName = Client::getName(%TrueClientId);
	else						%TCsenderName = %senderName;

	%time = getIntegerTime(true) >> 5;
	if(%time - %clientId.lastSayTime <= $sayDelay && !(%clientToServerAdminLevel >= 1))
		return;
	%clientId.lastSayTime = %time;

	%msg = %clientId @ " \"" @ escapeString(%message) @ "\"";


		// check for flooding if it's a broadcast OR if it's team in FFA
		if($Server::FloodProtectionEnabled && (!$Server::TourneyMode || !%team) && !(%clientToServerAdminLevel >= 1)) {
			%time = getIntegerTime(true) >> 5;
			if(%TrueClientId.floodMute) {
				%delta = %TrueClientId.muteDoneTime - %time;
				if(%delta > 0) {
					Client::sendMessage(%TrueClientId, $MSGTypeGame, "FLOOD! You cannot talk for " @ %delta @ " seconds.");
					return;
				}
				%TrueClientId.floodMute = "";
				%TrueClientId.muteDoneTime = "";
			}
			%TrueClientId.floodMessageCount++;
			schedule(%TrueClientId @ ".floodMessageCount--;", 5, %TrueClientId);
			if(%TrueClientId.floodMessageCount > 4) {
				%TrueClientId.floodMute = true;
				%TrueClientId.muteDoneTime = %time + 10;
				Client::sendMessage(%TrueClientId, $MSGTypeGame, "FLOOD! You cannot talk for 10 seconds.");
				return;
			}
		}
		// Updated exploit block thingies ported from main server
		if(string::len(%defaulttalk@" - "@%message) >= 200) {
			pecho("censor:"@%message);
			%message = "(length-censored)";
			%stringlength = string::len(%message);
		}
		if(String::findSubStr(%message, "\\n") != -1 || String::findSubStr(%message, "\\t") != -1 || String::findSubStr(%msg, "~)") != -1 || String::findSubStr(%msg, "\\x") != -1)
			%message = "(linebreak-censored)";
	}

	
		if(%clientId.currentShop != "" || %clientId.currentBank != "") {
			if(%message < 1)	%message = 1;
			if(%message > 100)	%message = 100;
		}
		%TrueClientId.bulkNum = %message;
	}

	//parse message
	%botTalk = False;
	%isCommand = False;

	if(String::getSubStr(%message, 0, 1) != "#") {
		if(%team)			%message = "#zone " @ %message;
		else				%message = fetchData(%TrueClientId, "defaultTalk") @ " " @ %message;
	}
	if(String::getSubStr(%message, 0, 1) == "#")
		%isCommand = True;

	if($exportChat) {
		%ip = Client::getTransportAddress(%TrueClientId);
		if(%TrueClientId.doExport) {
			$log::msg["[\"" @ %TCsenderName @ "\"]"] = %message;
			export("log::msg[\"" @ %TCsenderName @ "\"*", "temp\\log$ @ " @ %TCsenderName @ ".cs", true);
		}
	}

	%w1 = GetWord(%message, 0);

	if(fetchData(%TrueClientId, "BlockInputFlag") != "" && String::ICompare(%w1, "#endblock") != 0 && %w1 != -1 && %message != "") {
		//Entering block information into memory
		%tmpBlockCnt = fetchData(%TrueClientId, "tmpBlockCnt") + 1;
		storeData(%TrueClientId, "tmpBlockCnt", %tmpBlockCnt);
		$BlockData[%TCsenderName, fetchData(%TrueClientId, "BlockInputFlag"), %tmpBlockCnt] = %message;
		return 0;
	}

	%cropped = String::NEWgetSubStr(%message, (String::len(%w1)+1), 99999);

	if(%isCommand) {
		if ( processChat(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai) )				return;	
		if ( IsDead(%TrueClientId) && %TrueClientId != 2048 )																		return;

		if ( processPlayerCommands(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai) )	return;
		if ( processMinionControls(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai) )	return;
		if ( processThievery(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai) )			return;
		
		if(fetchData(%clientId, "invisible") != 2) {	// When Shadow Walking (perm stealth), disable Survival, Wordsmith, spells, and weapons 
			if ( processSurvival(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai) )		return;
			if ( processWordsmith(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai) )	return;
		}

		if ( processPackManagement(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai) )	return;		
		if ( processAdminLevel1(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai, %clientToServerAdminLevel) )	return;			
		if ( processAdminLevel2(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai, %clientToServerAdminLevel) )	return;			
		if ( processAdminLevel3(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai, %clientToServerAdminLevel) )	return;			
		if ( processAdminLevel4(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai, %clientToServerAdminLevel) )	return;			
		if ( processAdminLevel5(%clientId, %team, %message, %senderName, %w1, %cropped, %TrueClientId, %TCsenderName, %isai, %clientToServerAdminLevel) )	return;		
	}
	
	if(%botTalk) {
		processbottalk(%clientId,%TrueClientId,%message,%cropped,%w1);
		return;
	}
}

function remoteIssueCommand(%commander, %cmdIcon, %command, %wayX, %wayY, %dest1, %dest2, %dest3, %dest4, %dest5, %dest6, %dest7, %dest8, %dest9, %dest10, %dest11, %dest12, %dest13, %dest14) {
	for(%i = 1; %dest[%i] != ""; %i = %i + 1)
		if(!%dest[%i].muted[%commander])
			issueCommandI(%commander, %dest[%i], %cmdIcon, %command, %wayX, %wayY);
}

function remoteIssueTargCommand(%commander, %cmdIcon, %command, %targIdx, %dest1, %dest2, %dest3, %dest4, %dest5, %dest6, %dest7, %dest8, %dest9, %dest10, %dest11, %dest12, %dest13, %dest14) {
	for(%i = 1; %dest[%i] != ""; %i = %i + 1)
		if(!%dest[%i].muted[%commander])
			issueTargCommand(%commander, %dest[%i], %cmdIcon, %command, %targIdx);
}

function remoteCStatus(%clientId, %status, %message) {
	if(setCommandStatus(%clientId, %status, %message)) {
		if($dedicated)
			echo("COMMANDSTATUS: " @ %clientId @ " \"" @ escapeString(%message) @ "\"");
	}
	else internalSay(%clientId, true, %message);
}

function teamMessages(%mtype, %team1, %message1, %team2, %message2, %message3) {
	%numPlayers = getNumClients();
	for(%i = 0; %i < %numPlayers; %i = %i + 1) {
		%id = getClientByIndex(%i);
		if(Client::getTeam(%id) == %team1)							Client::sendMessage(%id, %mtype, %message1);		
		else if(%message2 != "" && Client::getTeam(%id) == %team2)	Client::sendMessage(%id, %mtype, %message2);		
		else if(%message3 != "")									Client::sendMessage(%id, %mtype, %message3);
	}
}

function messageAll(%mtype, %message, %filter) {
	dbecho($dbechoMode, "messageAll(" @ %mtype @ ", " @ %message @ ", " @ %filter @ ")");

	if(%filter == ""){
		for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
			Client::sendMessage(%cl, %mtype, %message);
	} else {
		for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl)) {
			if(%cl.messageFilter & %filter)
			Client::sendMessage(%cl, %mtype, %message);
		}
	}
}

function messageAllExcept(%except, %mtype, %message) {
	dbecho($dbechoMode, "messageAllExcept(" @ %except @ ", " @ %mtype @ ", " @ %message @ ")");

	for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl)) {
		if(%cl != %except)
			Client::sendMessage(%cl, %mtype, %message);
	}
}

function radiusAllExcept(%except1, %except2, %message, %battle) {
	dbecho($dbechoMode, "radiusAllExcept(" @ %except1 @ ", " @ %except2 @ ", " @ %message @ ")");

	%epos1 = GameBase::getPosition(%except1);
	%epos2 = GameBase::getPosition(%except2);
	for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl)) {
		%clpos = GameBase::getPosition(%cl);
		%dist1 = Vector::getDistance(%clpos, %epos1);
		%dist2 = Vector::getDistance(%clpos, %epos2);
		if(%cl != %except1 && %cl != %except2 && !IsDead(%cl)) {
			if(%dist1 <= $maxSAYdistVec || %dist2 <= $maxSAYdistVec) {
		}
	}
}

	if(Player::isAiControlled(%cl) || %cl < 2048)
		return;
		%message = String::replaceAll(%message, "<f2>", "");
		%message = String::replaceAll(%message, "<f3>", "");
		%message = String::replaceAll(%message, "<f4>", "");
		%message = String::replaceAll(%message, "<f5>", "");
	if(%cl.repack < 9){
			%cl.lastBMessage3 = "";%cl.lastBMessage2 = "";
		%cl.lastBMessage = %message @ "\n";
	else {
		if(%cl.repack >= 32){
		if(%cl.printlength == "")						%cl.printlength = 10;
	}

function FadeMsg(%txt, %dist, %max) {
	dbecho($dbechoMode, "FadeMsg(" @ %txt @ ", " @ %dist @ ", " @ %max @ ")");

	if(%dist <= %max) return %txt;
	else {
		for(%i = 0; (%z = GetWord(%txt, %i)) != -1; %i++)
			%ntxt = %ntxt @ %z;
		%lntxt = String::len(%ntxt);
		%x = %dist - %max;
		%amt = round((%x / %max) * %lntxt);
		%txt = BuildDotString(%txt, %amt);
		
		return %txt;
	}
}

function BuildDotString(%txt, %n) {
	dbecho($dbechoMode, "BuildDotString(" @ %txt @ ", " @ %n @ ")");

	%len = String::len(%txt);
	%retry = 0;
	for(%i = %n; %i > 0; %i) {
		%p = floor(getRandom() * %len);
		%a = String::getSubStr(%txt, %p, 1);
		if(%a != " " && %a != ".") {
			%txt = String::getSubStr(%txt, 0, %p) @ "." @ String::getSubStr(%txt, %p+1, 99999);
			%i--;
			%retry = 0;
		}
		else %retry++;

		if(%retry > 10)
			break;
	}
	return %txt;
}

function ClearBlockData(%name, %block) {
	dbecho($dbechoMode, "ClearBlockData(" @ %name @ ", " @ %block @ ")");
	for(%i = 1; $BlockData[%name, %block, %i] != ""; %i++)
		$BlockData[%name, %block, %i] = "";
}



function ParseBlockData(%bd, %victimId, %killerId) {
	dbecho($dbechoMode, "ParseBlockData(" @ %bd @ ", " @ %victimId @ ", " @ %killerId @ ")");

	//the passed variables MUST BE IN COMMALIST FORMAT!

	%vtype[1] = "^victimName";
	%vtype[2] = "^victimId";
	%vtype[3] = "^victimPos";
	%vtype[4] = "^victimRot";
	%vtype[5] = "^victimZoneId";
	%vtype[6] = "^victimZoneType";
	%vtype[7] = "^victimZoneDesc";
	%vtype[8] = "^victimClass";
	%vtype[9] = "^victimLevel";
	%vtype[10] = "^victimX";
	%vtype[11] = "^victimY";
	%vtype[12] = "^victimZ";
	%vtype[13] = "^victimR1";
	%vtype[14] = "^victimR2";
	%vtype[15] = "^victimR3";
	%vtype[16] = "^victimCoins";
	%vtype[17] = "^victimBank";
	%vtype[18] = "^victimVelX";
	%vtype[19] = "^victimVelY";
	%vtype[20] = "^victimVelZ";

	%vtype[21] = "^killerName";
	%vtype[22] = "^killerId";
	%vtype[23] = "^killerPos";
	%vtype[24] = "^killerRot";
	%vtype[25] = "^killerZoneId";
	%vtype[26] = "^killerZoneType";
	%vtype[27] = "^killerZoneDesc";
	%vtype[28] = "^killerClass";
	%vtype[29] = "^killerLevel";
	%vtype[30] = "^killerX";
	%vtype[31] = "^killerY";
	%vtype[32] = "^killerZ";
	%vtype[33] = "^killerR1";
	%vtype[34] = "^killerR2";
	%vtype[35] = "^killerR3";
	%vtype[36] = "^killerCoins";
	%vtype[37] = "^killerBank";
	%vtype[38] = "^killerVelX";
	%vtype[39] = "^killerVelY";
	%vtype[40] = "^killerVelZ";

	if(%victimId != "")
	{
		%vpos = GameBase::getPosition(%victimId);
		%vrot = GameBase::getRotation(%victimId);
		%vvel = Item::getVelocity(%victimId);

		%var[1] = Client::getName(%victimId);
		%var[2] = %victimId;
		%var[3] = %vpos;
		%var[4] = %vrot;
		%var[5] = fetchData(%victimId, "zone");
		%var[6] = Zone::getType(fetchData(%victimId, "zone"));
		%var[7] = Zone::getDesc(fetchData(%victimId, "zone"));
		%var[8] = fetchData(%victimId, "CLASS");
		%var[9] = fetchData(%victimId, "LVL");
		%var[10] = GetWord(%vpos, 0);
		%var[11] = GetWord(%vpos, 1);
		%var[12] = GetWord(%vpos, 2);
		%var[13] = GetWord(%vrot, 0);
		%var[14] = GetWord(%vrot, 1);
		%var[15] = GetWord(%vrot, 2);
		%var[16] = fetchData(%victimId, "COINS");
		%var[17] = fetchData(%victimId, "BANK");
		%var[18] = GetWord(%vvel, 0);
		%var[19] = GetWord(%vvel, 1);
		%var[20] = GetWord(%vvel, 2);
	}
	if(%killerId != "")
	{
		%kpos = GameBase::getPosition(%killerId);
		%krot = GameBase::getRotation(%killerId);
		%kvel = Item::getVelocity(%killerId);

		%var[21] = Client::getName(%killerId);
		%var[22] = %killerId;
		%var[23] = %kpos;
		%var[24] = %krot;
		%var[25] = fetchData(%killerId, "zone");
		%var[26] = Zone::getType(fetchData(%killerId, "zone"));
		%var[27] = Zone::getDesc(fetchData(%killerId, "zone"));
		%var[28] = fetchData(%killerId, "CLASS");
		%var[29] = fetchData(%killerId, "LVL");
		%var[30] = GetWord(%kpos, 0);
		%var[31] = GetWord(%kpos, 1);
		%var[32] = GetWord(%kpos, 2);
		%var[33] = GetWord(%krot, 0);
		%var[34] = GetWord(%krot, 1);
		%var[35] = GetWord(%krot, 2);
		%var[36] = fetchData(%killerId, "COINS");
		%var[37] = fetchData(%killerId, "BANK");
		%var[38] = GetWord(%kvel, 0);
		%var[39] = GetWord(%kvel, 1);
		%var[40] = GetWord(%kvel, 2);
	}

	for(%i = 1; %vtype[%i] != ""; %i++)
		%bd = String::replace(%bd, %vtype[%i], %var[%i], True);

	return %bd;
}




function charTranslate(%char){


//By phantom, tribesrpg.org
//added in v6.7.2
function string::printcolortag(%colour){

//By phantom, tribesrpg.org
//For use from console, not for writing into code.
function msg(%msg) {
	if(string::compare(%msg, "") == 0) {
		pecho("Allows speaking to players. ex: msg(\"Hi players!\");");
	} else {
		pecho("MSG - Console: " @ %msg);
	}
}