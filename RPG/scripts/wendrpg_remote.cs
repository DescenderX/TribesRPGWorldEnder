//This file is part of Tribes RPG.
//Tribes RPG server side scripts
//Written by Jason "phantom" Daley,  Matthiew "JeremyIrons" Bouchard, and more (yet undetermined)

//	Copyright (C) 2016  Jason Daley

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

function remoteTrue()
{
	//for some reason this function gets called from key binds, so i created it so the console doesn't get flooded with
	//remoteTrue: unknown commands.
	return;
}

//This fixes consol spam caused by idiots using poorly coded scripts :)
//This may be too simple.. Meh. Provided by tribesrpg.org
function remotePlayFakeDeath(){
}
function remoteSetclient(){
}
function remotegiveall(%client, %arg){
}
function remotebwadmin::isCompatible(%client, %arg){
}


function remotePlayMode(%clientId)
{
	Client::clearItemShopping(%clientId);
	Client::clearItemBuying(%clientId);
	ClearCurrentShopVars(%clientId);

	if(!%clientId.guiLock)
	{
		remoteSCOM(%clientId, -1);
		Client::setGuiMode(%clientId, $GuiModePlay);
	}
	if(%clientId.menuMode != ""){
		remoteScoresOff(%clientId);
	}
}

function remoteCommandMode(%clientId)
{
	//RPG players don't need commander mode.
	remoteRawKey(%clientId, "c");

	//if(!(%clientId.adminLevel >= 1))
	//{
	//	//RPG players don't need commander mode.
	//	return;
	//}

	Client::clearItemShopping(%clientId);
	Client::clearItemBuying(%clientId);
	ClearCurrentShopVars(%clientId);

	// can't switch to command mode while a server menu is up
	if(!%clientId.guiLock)
	{
		remoteSCOM(%clientId, -1);  // force the bandwidth to be full command

		Client::setGuiMode(%clientId, $GuiModeCommand);
	}
}

function remoteInventoryMode(%clientId)
{
	if(!%clientId.guiLock && !Observer::isObserver(%clientId))
	{
		remoteSCOM(%clientId, -1);
		Client::setGuiMode(%clientId, $GuiModeInventory);

		Client::clearItemShopping(%clientId);
		Client::clearItemBuying(%clientId);

		%txt = "<f1><jc>COINS: " @ fetchData(%clientId, "COINS");
		Client::setInventoryText(%clientId, %txt);
	}
}

function remoteObjectivesMode(%clientId)
{
	Client::clearItemShopping(%clientId);
	Client::clearItemBuying(%clientId);
	ClearCurrentShopVars(%clientId);

	if(!%clientId.guiLock)
	{
		remoteSCOM(%clientId, -1);
		Client::setGuiMode(%clientId, $GuiModeObjectives);
	}
}

function remoteScoresOn(%clientId)
{
	if(!%clientId.menuMode)
		Game::menuRequest(%clientId);
}

function remoteScoresOff(%clientId)
{
	Client::cancelMenu(%clientId);
}

function remoteToggleCommandMode(%clientId)
{
	if(Client::getGuiMode(%clientId) != $GuiModeCommand)
		remoteCommandMode(%clientId);
	else
		remotePlayMode(%clientId);
}

function remoteToggleInventoryMode(%clientId)
{
	Client::clearItemShopping(%clientId);
	Client::clearItemBuying(%clientId);
	ClearCurrentShopVars(%clientId);

	if(Client::getGuiMode(%clientId) != $GuiModeInventory)
		remoteInventoryMode(%clientId);
	else
		remotePlayMode(%clientId);
}

function remoteToggleObjectivesMode(%clientId)
{
	if(Client::getGuiMode(%clientId) != $GuiModeObjectives)
		remoteObjectivesMode(%clientId);
	else
		remotePlayMode(%clientId);
}

function remoteUseItem(%clientId, %type)
{
	dbecho($dbechoMode, "remoteUseItem(" @ %clientId @ ", " @ %type @ ")");

	%trueClientId = player::getclient(%clientId);

	%time = getIntegerTime(true) >> 5;
	if(%time - %trueClientId.lastWaitActionTime > $waitActionDelay)
	{
		%trueClientId.lastWaitActionTime = %time;

		%trueClientId.throwStrength = 1;

		%item = getItemData(%type);



		%pressedKey = -1;
		if(%item == Backpack) 
		{
			%item = -1;
			remoteConsider(%trueClientId);
			return;
		}
		%weaponNameToKey["Blaster"] = 1;

			if(%trueClientId.overrideKeybinds){
		else
		{
			if (%item == Weapon) 
				%item = Player::getMountedItem(%clientId,$WeaponSlot);
	
			if(%item != -1)
			{
				Player::useItem(%clientId, %item);
			}
		}
	}
}

function remoteThrowItem(%clientId,%type,%strength)
{
	%trueClientId = player::getclient(%clientId);
	%item = getItemData(%type);
	return;

	//echo("Throw item: " @ %type @ " " @ %strength);
	%item = getItemData(%type);
	if (%item == Grenade || %item == MineAmmo) {
		if (%strength < 0)
			%strength = 0;
		else
			if (%strength > 100)
				%strength = 100;
		%clientId.throwStrength = 0.3 + 0.7 * (%strength / 100);
		Player::useItem(%clientId,%item);
	}
}

function remoteDropItem(%clientId,%type)
{
	dbecho($dbechoMode, "remoteDropItem(" @ %clientId @ ", " @ %item @ ")");

	%time = getIntegerTime(true) >> 5;
	if(%time - %clientId.lastWaitActionTime > $waitActionDelay)
	{
		%clientId.lastWaitActionTime = %time;
	
		if((Client::getOwnedObject(%clientId)).driver != 1) {
			//echo("Drop item: ",%type);
			%clientId.throwStrength = 1;

			%item = getItemData(%type);
			if(%item == Weapon)
			{
				%item = Player::getMountedItem(%clientId,$WeaponSlot);
				Player::dropItem(%clientId,%item);
			}
			else if(%item == Ammo)
			{
				%item = Player::getMountedItem(%clientId,$WeaponSlot);
				if(%item.className == Weapon)
				{
					%item = %item.imageType.ammoType;
					Player::dropItem(%clientId,%item);
				}
			}
			else if (%item.className == Equipped)
			{
				Client::sendMessage(%clientId, $MsgRed, "You can't drop an equipped item!~wC_BuySell.wav");
			}
			else 
				Player::dropItem(%clientId,%item);
		}		
	}
}
function remoteDeployItem(%clientId,%type)
{
	//echo("Deploy item: ",%type);
	%item = getItemData(%type);
	Player::deployItem(%clientId,%item);
}

function remoteConsider(%clientId)
{
	dbecho($dbechoMode, "remoteConsider(" @ %clientId @ ")");

	if(%clientId.possessId != "") {
		rpg::RevertPossession(%clientId);
		return;
	}
	%object = fetchData(%clientId, "InEnterBox");
	%inSleepZone = fetchData(%clientId, "InSleepZone");

	%msgText[7] = "Easy prey!";
	%msgText[6] = "Shouldn't be a problem at all.";
	%msgText[5] = "You should win.";
	%msgText[4] = "Looks like an even fight.";
	%msgText[3] = "You might get killed...";
	%msgText[2] = "Looks VERY risky...";
	%msgText[1] = "You will NOT survive!";

	%msgColor[7] = $MsgGreen;
	%msgColor[6] = $MsgBeige;
	%msgColor[5] = $MsgBeige;
	%msgColor[4] = $MsgWhite;
	%msgColor[3] = $MsgRed;
	%msgColor[2] = $MsgRed;
	%msgColor[1] = $MsgRed;

	%maxMsg = 7;
	%midMsg = 4;
	%minMsg = 1;

	%nothingMsg = "You see nothing of interest.";
	%length = 500;
	%sawsomething = "";

	%player = Client::getOwnedObject(%clientId);
	%clientname = Client::getName(%clientId);
	%clientpos = GameBase::getPosition(%clientId);

	$los::object = "";
	$los::position = "";
	if(GameBase::getLOSinfo(%player, %length))
	{
		%object = $los::object;
		%objpos = $los::position;
		%obj = getObjectType(%object);
		%cl = Player::getClient(%object);

		%index = GetEventCommandIndex(%object.tag, "onConsider");

		if(%obj == "Player")
		{
			if( IsInCommaList(fetchData(%clientId, "PersonalPetList"), %cl) ||
				IsInCommaList(fetchData(%cl, "PersonalPetList"), %clientId) ) {
					rpg::PossessTarget(%clientId, %cl);
			} else {
				DisplayGetInfo(%clientId, %cl, %object);
				%sawsomething = True;
			}
		}
		else if($BotInfo[%object.name, NAME] != "")
			if($BotInfo[%object.name, NAME] != "Weather Device")
				if($TheMandateDeliveryTarget[rpg::getName(%clientId)] == %object)
					rpg::TheMandateDeliveryComplete(%clientId);
				else if($ShakedownTarget[rpg::getName(%clientId)] == %object) {
					GameBase::playSequence(%object, 0, "die head");
					$ShakedownTarget[rpg::getName(%clientId)] = "";
					storeData(%clientId,"COINS",$ShakedownWinnings[rpg::getName(%clientId)],"inc");
					Client::sendMessage(%clientId, $MsgGreen, "You found your shakedown target! " @ $BotInfo[%object.name, NAME] @ " is knocked out and drops " @ $ShakedownWinnings[rpg::getName(%clientId)] @ " coins!" );
					$ShakedownWinnings[rpg::getName(%clientId)] = "";
				}
					
				if($BotInfo[%object.name, NAME] != "Weather Device")
				else
					Client::sendMessage(%clientId, $MsgBeige, "You see a " @ $BotInfo[%object.name, NAME] @ ", but are too far away to inspect it." );
			}
		else if(%obj == "InteriorShape" && %object.tag != "" && %clientId.adminLevel >= 1)
		{
			Client::sendMessage(%clientId, $MsgWhite, %object @ "'s tag name: " @ %object.tag);
			%sawsomething = True;
		}
		else if(%clientId.adminLevel >= 3)
		{
			Client::sendMessage(%clientId, $MsgWhite, "Position at LOS is " @ %objpos);
			%sawsomething = True;
		}

		if(%index != -1)
		{
			%closest = 999999;
			%cindex = "";

			//pick the event with the closest radius, matching criteria of event
			for(%i2 = 0; (%index2 = GetWord(%index, %i2)) != -1; %i2++)
			{
				%ec = $EventCommand[%object.tag, %index2];

				%targetname = GetWord(%ec, 4);
				if(String::ICompare(%targetname, %clientname) == 0 || String::ICompare(%targetname, "all") == 0)
				{
					%radius = GetWord(%ec, 2);
					if(Vector::getDistance(%objpos, %clientpos) <= %radius)
					{
						if(%radius < %closest)
						{
							%closest = %radius;
							%cindex = %index2;
						}
					}
				}
			}

			if(%cindex != "")
			{
				%ec = $EventCommand[%object.tag, %cindex];

				%name = GetWord(%ec, 0);
				if((%cl = NEWgetClientByName(%name)) == -1)
					%cl = 2048;
				%keep = GetWord(%ec, 3);

				%cmd = String::NEWgetSubStr(%ec, String::findSubStr(%ec, ">")+1, 99999);
				%pcmd = ParseBlockData(%cmd, %clientId, "");
				if(!%keep)
					$EventCommand[%object.tag, %cindex] = "";
				internalSay(%cl, 0, %pcmd, %name);

				%sawsomething = True;
			}
		}
	}

	if(!%sawsomething)
		Client::sendMessage(%clientId, $MsgWhite, %nothingMsg);
}


function disableOverrides(%client){
	schedule::cancel("transportmenu"@%client);

function remoteRawOverride(%client, %key, %mod){
	if(string::getsubstr(%key, 0, 6) == "numpad" && string::len(%key) == 7)


//By phantom: tribesrpg.org
function remoteRawKey(%client, %key, %mod){
	if(%mod != "" && %mod != "control" && %mod != "alt" && %mod != "shift"){
		Client::sendMessage(%client, 0, "Erroneous modifier key.");
		return;
	}
	if(string::len(%key) > 15){
		Client::sendMessage(%client, 0, "Key name too long.");
		return;
	}
	if(String::findSubStr(%key, "\"") != -1 || String::findSubStr(%key, " ") != -1){
		client::sendmessage(%client, 0, "Invalid key.");
		return;
	}
	if(%client.isinvalid){
		client::sendmessage(%client, 0, "You have no character loaded yet!");
		return;
	}

	if(string::getsubstr(%key, 0, 1) == "f")
	{
		Client::sendMessage(%client, 0, "This key ("@%key@") is not yet supported.");
		return;
	}
	if($numMessage[%client, %key] == ""){
		Client::sendMessage(%client, 0, "#set "@%key@" [message]");
	}
	else{
		internalSay(%client,0,$numMessage[%client, %key]);
	}




	//client::sendmessage(%client, 0, "This server does not support the use of extra keybinds.");

	//Under normal conditions, %key will be one of the following:
	//Repack 4 and up:
	//"numpad0" - "numpad9", "numpadenter", "numpad+", "numpad-", "numpad*", "numpad/", "0"
	//Repack 6 adds:
	//"1" - "9" (only with %mod "alt" or "control"), "f1" - "f12" (only with %mod "")

	//Under normal conditions, %mod will be one of the following:
	//"", "control", "alt", "shift"

	//You shouldn't see "alt" and "numpadenter" together because that
	//toggles fullscreen, and thus isn't bound to this on the Tribes Repack.
	//If you decide to code something here, ensure that it can handle
	//anything a client might send to try to mess with the system.
	//See the current repack version's extra-controls.cs for a full list of
	//acceptable input, and note that this could be updated in the future.

}
function refreshBattleHudPos(%clientId){
	if(Player::isAiControlled(%clientId) || %clientId < 2048)
		return;
	

	remoteEval(%clientId, RPGMsgHudPos, %pos);