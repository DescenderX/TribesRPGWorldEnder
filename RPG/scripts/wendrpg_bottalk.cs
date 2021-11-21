//This file is part of Tribes RPG.
//Tribes RPG server side scripts
//New NPC Chat menu system written by Jason "phantom" Daley, tribesrpg.org

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

function NewBotMessage(%client, %closestId, %aimessage){//, %list){
	%clientId = %client;
	if(%client.tmpbottalk == "chat"){
		AI::sayLater(%client, %closestId, %aimessage, True);
		for(%i = 0; $botMenuOption[%client,%i] != ""; %i++)
			$botMenuOption[%client,%i] = "";
		%client.tmpbottalk = "";
		return;
	}

	%aiName = %closestId.name;
	if(%client.repack > 15)//Can have longer messages
		%menuname = "Conversation with "@$BotInfo[%ainame, NAME];
	else
		%menuname = $BotInfo[%ainame, NAME];

	if(%client.keyOverride != "bottalkChoice"){
		disableOverrides(%client);
		%client.keyOverride = "bottalkChoice";
		%client.overrideKeybinds = True;
	}
	%clientPos = GameBase::getPosition(%client);
	%botPos = GameBase::getPosition(%closestId);
	%closest = Vector::getDistance(%clientPos, %botPos);

	if(%closest > ($maxAIdistVec + 5))
	{
		$state[%closestId, %client] = "";
		endBotTalkChoice(%client);
		return;
	}

	%msg = %menuname@"\n<jc>"@%aimessage@"\n\n<f2>";

	%cnt = 0;
	for(%i = 0; $botMenuOption[%client,%i] != ""; %i++)
	{
		%trigger = $botMenuOption[%client,%i];

		if((%break = string::findsubstr(%trigger, "|")) > 0){
			%trigger1 = string::getsubstr(%trigger, 0, %break);
			%trigger2 = string::getsubstr(%trigger, %break+1, 999);
		}
		else{
			%trigger1 = %trigger;
			%trigger2 = %trigger;
		}
	}
	%msg = %msg @ "\n\n0: Close menu.";
	rpg::longPrint(%client,%msg,1,0.7);

	%client.curNPC = %closestId;
	%aiMessage = escapeString(%aiMessage);
	schedule::add("NewBotMessage("@%client@","@%closestId@",\""@%aimessage@"\");",0.4,"NewBotMessage"@%client);
}


function endBotTalkChoice(%client){
	%client.curNPC = "";
	bottomPrint(%client,"",0);
	disableOverrides(%client);
	for(%i = 0; $botMenuOption[%client,%i] != ""; %i++)
	{
		$botMenuOption[%client,%i] = "";
	}
	$yousaid[%client] = "";
}


function bottalkChoice(%client,%key){

	if(%key == 0){
		endBotTalkChoice(%client);
		return;
	}
		%object = %client.curNPC;
		for(%i = 0; $botMenuOption[%client,%i] != ""; %i++)
		{
			%trigger = $botMenuOption[%client,%i];
			if((%break = string::findsubstr(%trigger, "|")) > 0){
				%trigger1 = string::getsubstr(%trigger, 0, %break);
				%trigger2 = string::getsubstr(%trigger, %break+1, 999);
			}
			else{
				%trigger1 = %trigger;
				%trigger2 = %trigger;
			}
			%cnt++;
			if(%cnt == %key){
				%msg = %trigger2;
				%validOption = True;
				break;
			}
		}

	if(%validOption){
		endBotTalkChoice(%client);
		$yousaid[%client] = %msg;		
		if($TheMandateDeliveryTarget[rpg::getName(%client)] == %object)
			rpg::TheMandateDeliveryComplete(%client);
		eval("bottalk::"@clipTrailingNumbers(%object.name)@"("@%client@","@%object@",False,\"#say "@%msg@"\");");
	}
	else{
		if($BotInfo[%object.name, NAME] != "Weather Device")
			AI::sayLater(%client, %object, "What was that?", True);
	}
}



function processbottalk(%clientId,%TrueClientId,%message,%cropped,%w1){


	//process TownBot talk

	%initTalk = False;
	for(%i = 0; (%w = GetWord("hail hello hi greetings yo hey sup salutations g'day howdy", %i)) != -1; %i++)
		if(String::ICompare(%cropped, %w) == 0)
			%initTalk = True;

	%clientPos = GameBase::getPosition(%TrueClientId);
	%closest = 5000000;

	for(%i = 0; (%id = GetWord($TownBotList, %i)) != -1; %i++)
	{
		%botPos = GameBase::getPosition(%id);
		%dist = Vector::getDistance(%clientPos, %botPos);

		if(%dist < %closest)
		{
			%closest = %dist;
			%closestId = %id;
			%closestPos = %botPos;
		}
	}


	%aiName = %closestId.name;
	%displayName = $BotInfo[%aiName, NAME];




	if(%closest <= ($maxAIdistVec + 5) && Client::getTeam(%TrueClientId) == GameBase::getTeam(%closestId))
	{

		if(%TrueClientId.curNPC != "")
			endBotTalkChoice(%TrueClientId);
		if(%initTalk)
		{
			//Rotate Bot to look at player
			%rot = Vector::getRotation(Vector::normalize(Vector::sub(%clientPos, %closestPos)));
			%rot = "0 -0 "@GetWord(%rot, 2);
			GameBase::setRotation(%closestId, %rot);
		}

		if(String::findSubStr(%cropped, "\"") != -1){
			return;
		}

		%TrueClientId.tmpbottalk = "chat";
		%fname = clipTrailingNumbers(%aiName);		
		if($TheMandateDeliveryTarget[rpg::getName(%TrueClientId)] == %closestId)
			rpg::TheMandateDeliveryComplete(%TrueClientId);
		eval("bottalk::"@%fName@"("@%TrueClientId@","@%closestId@","@%initTalk@",\""@escapestring(%message)@"\");");
	}
	else
	{
		//This condition occurs when you are talking from too far of any TownBot.  All states are cleared here.
		//This means that potentially, you could initiate a conversation with the banker, travel for an hour
		//WITHOUT saying a word, come back and continue the conversation.  As soon as you speak in a way that
		//townbots hear you (#say, #shout, #tell) and are too far from them, all conversations are reset.

		//This is old code but I am leaving it in just because it could still be useful.

		for(%i = 0; (%id = GetWord($TownBotList, %i)) != -1; %i++)
			$state[%id, %TrueClientId] = "";
	}

}

function bottalk::merchant(%TrueClientId, %closestId, %initTalk, %message){
	//process merchant code
	%trigger[2] = "buy";

	if(%initTalk)
	{
		$botMenuOption[%TrueClientId,0] = "I would like to buy something.";
		NewBotMessage(%TrueClientId, %closestId, "Did you come to see what items you can buy?");
		$state[%closestId, %TrueClientId] = 1;
	}
	else if($state[%closestId, %TrueClientId] == 1)
	{
		if(String::findSubStr(%message, %trigger[2]) != -1)
		{
			SetupShop(%TrueClientId, %closestId);
			AI::sayLater(%TrueClientId, %closestId, "Take a look at what I have.", True);
			$state[%closestId, %TrueClientId] = "";
		}
	}
}

function bottalk::luckvendor(%TrueClientId, %closestId, %initTalk, %message){
	%w1 = GetWord(%message, 0);
	%cropped = String::NEWgetSubStr(%message, (String::len(%w1)+1), 99999);

	if(%initTalk)
	{
		$botMenuOption[%TrueClientId,0] = "trinket";
		$botMenuOption[%TrueClientId,1] = "buy";
		NewBotMessage(%TrueClientId, %closestId, "How is your FAVOR with the gods? I have trinkets for sale that are sure to help...");
		$state[%closestId, %TrueClientId] = 1;
	}
	else if($state[%closestId, %TrueClientId] == 1)
	{
		if(String::findSubStr(%message, "trinket") != -1)
		{
			%cost = GetFavorGoldCost(%TrueClientId);
			$botMenuOption[%TrueClientId,0] = "yes";
			$botMenuOption[%TrueClientId,1] = "no";
			NewBotMessage(%TrueClientId, %closestId, "This trinket will give you FAVOR. " @ %cost @ " coins will cover my \"costs\". Do we have a deal? (YES/NO)");
			$state[%closestId, %TrueClientId] = 2;
		} 
		else if(String::findSubStr(%message, "buy") != -1) {
			bottalk::SetupShop(%TrueClientId, %closestId, "", %w1);
			return;
		}
	}
	else if($state[%closestId, %TrueClientId] == 2)
	{
		if(String::findSubStr(%message, "yes") != -1)
		{
			%cost = GetFavorGoldCost(%TrueClientId);
			if(fetchData(%TrueClientId, "COINS") >= %cost)
			{
				NewBotMessage(%TrueClientId, %closestId, "Excellent. Here is your trinket...");
				GiveThisStuff(%TrueClientId, "FAVOR 1", True);
				storeData(%TrueClientId, "COINS", %cost, "dec");
				RefreshAll(%TrueClientId);
			}
			else
				NewBotMessage(%TrueClientId, %closestId, "Feel free to return - when you actually have enough coin.");
			$state[%closestId, %TrueClientId] = "";
		}
		else if(String::findSubStr(%message, "no") != -1)
		{
			AI::sayLater(%TrueClientId, %closestId, "Goodbye.", True);
			$state[%closestId, %TrueClientId] = "";
		}
	}
}





// See bottalk_strings.cs for clear usage examples. Abuses one 3d array with two forms
// A barebones dialog tree handler was needed. Abuses 3D arrays. 
//		["CUE_Name", 	<reserved>, 	"keyword"]	= "TEXT"			-> When KEYWORD is heard, the bot processes these actions and shows the TEXT
//		["CUE_Name", 	"keyword", 		index]		= "NEXT_KEYWORD"	-> NEXT_KEYWORD sets up the player's potential responses to the bot's KEYWORD TEXT
function bottalk::info(%TrueClientId, %closestId, %initTalk, %message){
	%aiName = %closestId.name;

	%chatname = $BotInfo[%aiName, CUE, 1];
	if(%chatname == "") return;
	%keyword = GetWord(%message, 1);

	%typeToken 		= "! ~! + ~+";
	%redirectToken 	= "= ~= - ~-";
	for(%tok=0;%tok<4;%tok++) {
		%tt = getword(%typeToken,%tok);
		%rt = getword(%redirectToken,%tok);

		for(%x=0;%x<10;%x++) {
			%check 		= %tt;
			%gotoword 	= %rt;
			if(%x > 0) {
				%check 		= %check @ %x;
				%gotoword 	= %gotoword @ %x;
			}
			%stuff 		= $BotInfoChat[%chatname, %check, %keyword];
			%redirect 	= $BotInfoChat[%chatname, %gotoword, %keyword];
			if(%stuff != "" && %redirect != "") {
				if(%tok == 0 || %tok == 2) {
					%theStuff = HasThisStuff(%TrueClientId, %stuff);
					if(%tok == 0 && %theStuff <= 0) {
						bottalk::info(%TrueClientId, %closestId, %initTalk, getword(%message,0) @ " " @ %redirect);
						return;
					} else if(%tok == 2 && %theStuff > 0 && %theStuff != 667 && %theStuff != 666) {
						bottalk::info(%TrueClientId, %closestId, %initTalk, getword(%message,0) @ " " @ %redirect);
						return;
					}
				} else if(%tok == 1 || %tok == 3) {
					%theStuff = HasSomeOfThisStuff(%TrueClientId, %stuff);
					if(%tok == 1 && %theStuff <= 0) {
						bottalk::info(%TrueClientId, %closestId, %initTalk, getword(%message,0) @ " " @ %redirect);
						return;
					} else if(%tok == 3 && %theStuff > 0 && %theStuff != 667 && %theStuff != 666) {
						bottalk::info(%TrueClientId, %closestId, %initTalk, getword(%message,0) @ " " @ %redirect);
						return;
					}
				}
			}
		}
	}
	
	%response 			= "";
	%staticParameters 	= "";
	%evalLine 			= $BotInfoChat[%chatname, EVAL,  %keyword];
	%response 			= $BotInfoChat[%chatname, SAY, %keyword];
	if(%evalLine != "") {
		%fnToExec = getword(%evalLine,0);

		if(%fnToExec != %evalLine) {			
			%staticParameters = string::newGetSubStr(%evalLine,string::len(%fnToExec),9999);
		}
		%fnCall = %fnToExec @ "(" @ %TrueClientId @ "," @ %closestId @ ", \"" @ %response @ "\", \"" @ %keyword @ "\"" @ %staticParameters @ ");";
		
		%newKeyword = eval( %fnCall );
		
		if(%newKeyword == -1) {
			bottalk::info(%TrueClientId, %closestId, %initTalk, getword(%message,0) @ " hello");
			return;
		} else if(%newKeyword == -2) {
			$state[%closestId, %TrueClientId] = "";
			return;
		} else if((%isNewResponse=String::findSubStr(%newKeyword,"+")) == 0) {
			%response = String::newGetSubStr(%newKeyword,1,99999);
		} else if(%newKeyword != "") {
			bottalk::info(%TrueClientId, %closestId, %initTalk, getword(%message,0) @ " " @ %newKeyword);
			return;
		}
	}
	if(%response != "") {
		$state[%closestId, %TrueClientId] = "";
		NewBotMessage(%TrueClientId, %closestId, %response, True);		
		for(%x=0;%x<10;%x++) {			
			$botMenuOption[%TrueClientId,%x] = "";
			%menuOption = $BotInfoChat[%chatname, %keyword, %x];
			if(%menuOption == "") break;
			$botMenuOption[%TrueClientId,%x] = %menuOption;
		}
		if($botMenuOption[%TrueClientId,0] != "")
			$state[%closestId, %TrueClientId] = 1;			// if there's potentially more to chat about, keep the talking state active
	}
}

function bottalk::SetupShop(%clientId, %botId, %w1, %keyword)  {
	%aiName = %botId.name;
	if($BotInfo[%aiName, SHOP] != "" || $BotInfo[%aiName, BELTSHOP] != "") {
		SetupShop(%clientId, %botId);
		AI::sayLater(%clientId, %botId, "Take a look at what I have.", True);
		$state[%botId, %clientId] = "";
		return -2;
	}
	return "";
}

function bottalk::quest(%TrueClientId, %closestId, %initTalk, %message){
	%aiName = %closestId.name;
	//process quest code
	%trigger[2] = $BotInfo[%aiName, CUE, 1];
	%trigger[3] = $BotInfo[%aiName, NCUE, 1];
	%trigger[4] = "buy";

	%hasTheStuff = HasThisStuff(%TrueClientId, $BotInfo[%aiName, NEED]);

	if($BotInfo[%aiName, CSAY] == "" && %hasTheStuff == 666)
		%hasTheStuff = False;
	if($BotInfo[%aiName, LSAY] == "" && %hasTheStuff == 667)
		%hasTheStuff = False;


	if(%hasTheStuff == 666 && %initTalk)// $state[%closestId, %TrueClientId] == "")
	{
		NewBotMessage(%TrueClientId, %closestId, $BotInfo[%aiName, CSAY], True);
		$state[%closestId, %TrueClientId] = -5;
	}
	else if(%hasTheStuff == 667 && %initTalk)// $state[%closestId, %TrueClientId] == "")
	{
		NewBotMessage(%TrueClientId, %closestId, $BotInfo[%aiName, LSAY], True);
		$state[%closestId, %TrueClientId] = -5;
	}
	else if(%hasTheStuff == False)
	{
		if(%initTalk)
		{
			$botMenuOption[%TrueClientId,0] = %trigger[2];
			$botMenuOption[%TrueClientId,1] = "Have anything for me to buy?";
			NewBotMessage(%TrueClientId, %closestId, $BotInfo[%aiName, SAY, 1] @ " [" @ %trigger[2] @ "]", True);
			$state[%closestId, %TrueClientId] = 1;
		}
		else if($state[%closestId, %TrueClientId] == 1)
		{
			if(String::findSubStr(%message, %trigger[2]) != -1)
			{
				NewBotMessage(%TrueClientId, %closestId, $BotInfo[%aiName, SAY, 2], True);
				$state[%closestId, %TrueClientId] = "";
			}
		}
	}
	else if(%hasTheStuff == True)
	{
		if(%initTalk)
		{
			$botMenuOption[%TrueClientId,0] = %trigger[3];
			$botMenuOption[%TrueClientId,1] = "Have anything for me to buy?";
			NewBotMessage(%TrueClientId, %closestId, $BotInfo[%aiName, NSAY, 1] @ " [" @ %trigger[3] @ "]", True);
			$state[%closestId, %TrueClientId] = 1;
		}
		else if($state[%closestId, %TrueClientId] == 1)
		{
			if(String::findSubStr(%message, %trigger[3]) != -1)
			{
				if(HasThisStuff(%TrueClientId, $BotInfo[%aiName, NEED]))
				{
					if($BotInfo[%aiName, TAKE] != "")
						TakeThisStuff(%TrueClientId, $BotInfo[%aiName, TAKE], True);
					if($BotInfo[%aiName, GIVE] != "")
						GiveThisStuff(%TrueClientId, $BotInfo[%aiName, GIVE], True);

					NewBotMessage(%TrueClientId, %closestId, $BotInfo[%aiName, NSAY, 2], True);
				}
				else
					AI::sayLater(%TrueClientId, %closestId, "Nice try, I'm keeping what I managed to get from you.", True);
	
				$state[%closestId, %TrueClientId] = "";
				Game::refreshClientScore(%TrueClientId);
			}
		}
	}
}


	%aiName = %closestId.name;
			$botMenuOption[%TrueClientId,8] = "brawler";
		else if(String::ICompare(%class, "brawler") == 0)	%class = "Brawler";
				if(getRandom() > 0.6) %gender = "Female";
				%names["Male"] = "Maximus Geronimo Avatar Necron Abacus Ternim Unther Deytuv Kessam Henry Slivat Pronimus Fennis Mortyr Vlad Xavius Tuvan";
				%names["Female"] = "Maxi Sherry Avoa Liera Varia Edene Raela Fenna Star Kondorah Ankana";
				if(%class == "Cleric" || %class == "Bard" || %class == "Mage" || %class == "Druid") {
					%armor[0] = "FineRobe";
					%armor[1] = "RobeOfOrder";
					%armor[2] = "BloodRobe";
					%armor[3] = "RobeOfVenjance";
				} else {
					%armor[0] = "HideArmor";
					%armor[1] = "RingMail";
					%armor[2] = "BandedMail";
					%armor[3] = "SpikedLeather";
				}
				%armor = %armor[floor(getRandom() * 4)];
				%pos = Vector::Add(GameBase::getPosition(%TrueClientId), RandomPositionXY(5,10) @ "0");

				if(%class == "Cleric" || %class == "Bard" || %class == "Mage" || %class == "Druid") 
				else ChangeRace(%id, %gender @ "Human");
				storeData(%id, "noDropLootbagFlag", true);

function bottalk::blacksmith(%TrueClientId, %closestId, %initTalk, %message){
	%aiName = %closestId.name;
	//process smith code
	%trigger[2] = "buy";
	%trigger[3] = "smith";

	if(%initTalk)
	{
		if($BotInfo[%aiName, SHOP] != "")
			$botMenuOption[%TrueClientId,0] = "Buy";
		NewBotMessage(%TrueClientId, %closestId, "Hail friend, look at the anvil and say #smith to smith things.");
		$state[%closestId, %TrueClientId] = 1;

		//We stop using the blacksmith because it isn't belt-compatible.
		return;
		$botMenuOption[%TrueClientId,0] = "Yeah I'd like to smith.";
		NewBotMessage(%TrueClientId, %closestId, "Hail friend, are you here to have me SMITH an old weapon?");
		$state[%closestId, %TrueClientId] = 1;
	}
	else if($state[%closestId, %TrueClientId] == 1)
	{
		if(String::findSubStr(%message, %trigger[2]) != -1)
		{
			if($BotInfo[%aiName, SHOP] != "")
			{
				SetupShop(%TrueClientId, %closestId);
				AI::sayLater(%TrueClientId, %closestId, "Take a look at what I have.", True);
			}
			else
				NewBotMessage(%TrueClientId, %closestId, "I have nothing to sell.");
			$state[%closestId, %TrueClientId] = "";
		}
		//We stop using the blacksmith because it isn't belt-compatible.
		return;
		if(String::findSubStr(%message, %trigger[3]) != -1)
		{
			AI::sayLater(%TrueClientId, %closestId, "Click Use on an item and I will tell you how much it will cost to smith. Click Use on this item again and I will get to work.", True);
			SetupBlacksmith(%TrueClientId, %closestId);
			$state[%closestId, %TrueClientId] = "";
		}
	}
}

		$botMenuOption[%TrueClientId,0] = "fight";
		$botMenuOption[%TrueClientId,1] = "no";
			if(%x != -1) 	TeleportToMarker(%TrueClientId, "TheArena\\WaitingRoomMarkers", 0, 1);
			$state[%closestId, %TrueClientId] = "";
			if(GameBase::getMapName(%closestId) != "Boss Man") {
				NewBotMessage(%TrueClientId, %closestId, "Hah! No? Creten demands entertainment! You'll fight, or die here.", "");
			} else {
			}
			$state[%closestId, %TrueClientId] = "";