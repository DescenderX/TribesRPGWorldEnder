//This file is part of Tribes RPG.
//Tribes RPG server side scripts
//Written by Jason "phantom" Daley,  Matthiew "JeremyIrons" Bouchard, and more (yet undetermined)

//	Copyright (C) 2014  Jason Daley

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

//_______________________________________________________________________________________________________________________________
// DescX Notes:
//		Describing changes here would be too hard. They are frequent and many. Mostly for gameplay reasons; some bug fixing;
//		and some useful missing routines.
//
//		See the bottom of this file for "new" stuff I've added. 

function fetchData(%clientId, %type)
{
	dbecho($dbechoMode, "fetchData(" @ %clientId @ ", " @ %type @ ")");

	if(%type == "LVL")
	{
		%a = GetLevel(fetchData(%clientId, "EXP"), %clientId);
		return %a;
	}
	else if(%type == "DEF")
	{
		%a = AddPoints(%clientId, 7);
		%b = AddBonusStatePoints(%clientId, "DEF");
		if(!IsDead(%clientId)){
			if(Player::getItemCount(%clientId, SlayerGear0)) {
				%b += Cap(rpg::GetHouseLevel(%clientId) * 8, 100, 750);
			}
		}
		%c = (%a + %b);
		if(Player::isAiControlled(%clientId)) {
			%c += floor(GetSkillWithBonus(%clientId,$SkillEndurance) / 100.0 * fetchData(%clientId,"LVL") * 0.1);
		}
		%d = (fetchData(%clientId, "OverweightStep") * 7.0) / 100;
		%e = Cap(%c - (%c * %d), 0, "inf");
		
		return floor(%e);
	}
	else if(%type == "MDEF")
	{
		%a = AddPoints(%clientId, 3);
		%b = AddBonusStatePoints(%clientId, "MDEF");
		%c = (%a + %b);
		if(Player::isAiControlled(%clientId)) {
			%c += floor(GetSkillWithBonus(%clientId,$SkillWillpower) / 100.0 * fetchData(%clientId,"LVL") * 0.33);
		}
		%d = (fetchData(%clientId, "OverweightStep") * 7.0) / 100;
		%e = Cap(%c - (%c * %d), 0, "inf");
		
		return floor(%e);
	}
	else if(%type == "ATK")
	{
		%weapon = Player::getMountedItem(%clientId, $WeaponSlot);

		if(%weapon != -1)
		{
			%a = AddBonusStatePoints(%clientId, "ATK");
			%a += GetWord(GetAccessoryVar(%weapon, $SpecialVar), 1);
			if(GetAccessoryVar(%weapon, $AccessoryType) == $RangedAccessoryType) {
				%weapon = fetchData(%clientId, "LoadedProjectile " @ %weapon);
				%a += GetWord(GetAccessoryVar(%weapon, $SpecialVar), 1);
			}
			return %a;
		} else return 0;
	}
	else if(%type == "MaxHP")
	{
		%sacrificeBonus = AddBonusStatePoints(%clientId,"SCRFC");
		if(%sacrificeBonus <= 0)
			%sacrificeBonus = 1.0;
		
		// Class multiplier X 12 X Level + Vit
		%remortCount 		= Cap(fetchData(%clientId, "RemortStep"), 0, 100);
		%hpPerVit 			= $MinHP[fetchData(%clientId, "RACE")] * Cap(GetSkillMultiplier(%clientId, $SkillVitality), 0.5, 1.0);		
		%hpPerLvl 			= %hpPerVit * fetchData(%clientId, "LVL") / 2;
		%hpVitBonus	 		= GetSkillWithBonus(%clientId, $SkillVitality) * (($MinHP[fetchData(%clientId, "RACE")] + %remortCount) / 10);
		%hpRemortBonus		= %hpPerVit * Cap(fetchData(%clientId, "RemortStep"), 0, 100);	
		%hpItemBonus	 	= AddPoints(%clientId, 4);										
		%hpBuffBonus 		= AddBonusStatePoints(%clientId, "MaxHP");			

		return floor(($MinHP[fetchData(%clientId, "RACE")] + %hpPerLvl + %hpVitBonus + %hpRemortBonus + %hpItemBonus + %hpBuffBonus + 1) * %sacrificeBonus);
	}
	else if(%type == "HP")
	{
		%armor = Player::getArmor(%clientId);

		%c = %armor.maxDamage - GameBase::getDamageLevel(Client::getOwnedObject(%clientId));
		%a = %c * fetchData(%clientId, "MaxHP");
		%b = %a / %armor.maxDamage;

		return round(%b);
	}
	else if(%type == "MaxMANA")
	{
		%energySkill	= GetSkillMultiplier(%clientId, $SkillEnergy);
		%remortCount 	= Cap(fetchData(%clientId, "RemortStep"), 0, 100);
		%mpPerVit 		= Cap((4 + %remortCount) * Cap(%energySkill, 0.2, 1.0), 1, 10);
		%mpPerLvl 		= %mpPerVit * fetchData(%clientId, "LVL");
		%mpVitBonus 	= GetSkillWithBonus(%clientId, $SkillEnergy) * Cap(%energySkill, 0.1, 1.5);
		%mpItemBonus 	= AddPoints(%clientId, 5);
		%mpBuffBonus 	= AddBonusStatePoints(%clientId, "MaxMANA");

		return floor(%mpPerLvl + %mpVitBonus + %mpRemortBonus + %mpItemBonus + %mpBuffBonus);
	}
	else if(%type == "MANA")
	{
		%armor = Player::getArmor(%clientId);

		%a = GameBase::getEnergy(Client::getOwnedObject(%clientId)) * fetchData(%clientId, "MaxMANA");
		%b = %a / %armor.maxEnergy;

		return round(%b);
	}
	else if(%type == "MaxWeight")
	{
		%a = 50 + GetSkillWithBonus(%clientId, $SkillEndurance);
		if(!IsDead(%clientId)){
			if(Player::getItemCount(%clientId, SlayerGear0)) {
				%a *= Cap(rpg::GetHouseLevel(%clientId) / 18, 1, 8);
			}
		}
		%c = AddBonusStatePoints(%clientId, "MaxWeight");
		%falseWeight = AddBonusStatePoints(%clientId, "WEIGHT");
		if(%falseWeight < 0) %falseWeight = -%falseWeight;
		else %falseWeight = 0;

		return RoundToFirstDecimal(%a + %c + %falseWeight);
	}
	else if(%type == "Weight")
	{
		return GetWeight(%clientId);
	}
	else if(%type == "RankPoints")
	{
		return Cap(floor($ClientData[%clientId, %type]), 0, "inf");
	}
	else if(%type == "OverweightStep")
	{
		return Cap(floor($ClientData[%clientId, %type]), 0, "inf");
	}
	else if(%type == "SlowdownHitFlag")
	{
		if(Player::isAiControlled(%clientId))
			return False;
		else
			return $ClientData[%clientId, %type];
	}
	else
		return $ClientData[%clientId, %type];

	return False;
}
function remotefetchData(%clientId, %type)
{
	dbecho($dbechoMode, "remotefetchData(" @ %clientId @ ", " @ %type @ ")");

	if(%clientId.isinvalid)
		return;

	//rpgfetchdata specific vartypes
	if(%type == "zonedesc")
	{
		%r = fetchData(%clientId, "zone");
		%data = Zone::getDesc(%r);
	}
	else if(string::icompare(%type, "password") == 0)
	{
		return;
	}
	else if(%type == "servername")
	{
		%data = $Server::HostName;
	}
	else if(GetWord(%type, 0) == "skill" && (%s = GetWord(%type, 1)) != -1)
	{
		return;
		%data = $PlayerSkill[%clientId, %s];
	}
	else if(GetWord(%type, 0) == "getbuycost" && (%s = GetWord(%type, 1)) != -1)
	{
		return;
		%data = getBuyCost(%clientId, %s);
	}
	else if(GetWord(%type, 0) == "getsellcost" && (%s = GetWord(%type, 1)) != -1)
	{
		return;
		%data = getSellCost(%clientId, %s);
	}
	else if(GetWord(%type, 0) == "skillcanuse" && (%s = GetWord(%type, 1)) != -1)
	{
		return;
		%data = SkillCanUse(%clientId, %s);
	}
	else if(GetWord(%type, 0) == "spellcancast" && (%s = GetWord(%type, 1)) != -1)
	{
		return;
		%data = SpellCanCast(%clientId, %s);
	}
	else if(GetWord(%type, 0) == "skillcancastnow" && (%s = GetWord(%type, 1)) != -1)
	{
		return;
		%data = SpellCanCastNow(%clientId, %s);
	}
		%data = -1;
		if(!IsDead(%clientId)) {
		}
	else if(%type == "RACE" || %type == "CLASS" || %type == "EXP" || %type == "FAVOR" || 
			%type == "COINS" || %type == "MANA" || %type == "RemortStep" || %type == "bounty" || 
			%type == "RankPoints" || %type == "MyHouse" || %type == "HP" || %type == "MaxHP" || 
			%type == "BANK" || %type == "DEF" || %type == "MDEF" || %type == "SPcredits" || 
			%type == "isMimic" || %type == "ATK" || %type == "MaxMANA" || %type == "MaxWeight" || 
			%type == "FAVORmode" || %type == "Weight" || %type == "LVL" || %type == "grouplist")
		%data = fetchData(%clientId, %type);
	else
	{
		%data = "omg!";
	}

	remoteEval(%clientId, SetRPGdata, %data, %type);
}

function storeData(%clientId, %type, %amt, %special)
{
	dbecho($dbechoMode, "storeData(" @ %clientId @ ", " @ %type @ ", " @ %amt @ ", " @ %special @ ")");

	if(%type == "HP")
	{
		setHP(%clientId, %amt);
	}
	else if(%type == "MANA")
	{
		setMANA(%clientId, %amt);
	}
	else if(%type == "MaxHP" || %type == "MaxMANA" || %type == "MaxWeight" || %type == "Weight")
	{
		echo("Invalid call to storeData for " @ %type @ " : Can't manually set this variable.");
	}
	else
	{
		if(%special == "inc")
			$ClientData[%clientId, %type] += %amt;
		else if(%special == "dec")
			$ClientData[%clientId, %type] -= %amt;
		else if(%special == "strinc")
			$ClientData[%clientId, %type] = $ClientData[%clientId, %type] @ %amt;
		else
			$ClientData[%clientId, %type] = %amt;

		if(GetWord(%special, 1) == "cap")
			$ClientData[%clientId, %type] = Cap($ClientData[%clientId, %type], GetWord(%special, 2), GetWord(%special, 3));
	}
}

function MenuSP(%clientId, %page)
{
	dbecho($dbechoMode, "MenuSP(" @ %clientId @ ", " @ %page @ ")");

	Client::buildMenu(%clientId, "You have " @ fetchData(%clientId, "SPcredits") @ " SP credits", "sp", true);

	%clientId.bulkNum = "";

	%l = 6;
	%ns = GetNumSkills();
	%np = floor(%ns / %l);
	
	%lb = (%page * %l) - (%l-1);
	%ub = %lb + (%l-1);
	if(%ub > %ns)
		%ub = %ns;

	

	for(%i = %lb; %i <= %ub; %i++) {
		%prog = floor(rpg::SkillAdvancementProgress(%clientId,%i));
		Client::addMenuItem(%clientId, %cnt++ @ "| " @ GetPlayerSkill(%clientId, %i) @ " | " @ $SkillDesc[%i] @ " (" @ %prog @ "%)", %i @ " " @ %page);
	}

	if(%page == 1)
	{
		Client::addMenuItem(%clientId, "nNext >>", "page " @ %page+1);
		Client::addMenuItem(%clientId, "bBack", "done");
	}
	else if(%page == %np+1)
	{
		Client::addMenuItem(%clientId, "p<< Prev", "page " @ %page-1);
		Client::addMenuItem(%clientId, "bBack", "done");
	}
	else
	{
		Client::addMenuItem(%clientId, "nNext >>", "page " @ %page+1);
		Client::addMenuItem(%clientId, "p<< Prev", "page " @ %page-1);
		Client::addMenuItem(%clientId, "bBack", "done");
	}

	return;
}
function processMenusp(%clientId, %opt)
{
	dbecho($dbechoMode, "processMenusp(" @ %clientId @ ", " @ %opt @ ")");

	%o = GetWord(%opt, 0);
	%p = GetWord(%opt, 1);

	if(fetchData(%clientId, "SPcredits") > 0 && %o != "page" && %o != "done")
	{
		if(%clientId.bulkNum < 1)
			%clientId.bulkNum = 1;
		if(%clientId.bulkNum > 30 && !(%clientId.adminLevel >= 1) )
			%clientId.bulkNum = 30;

		for(%i = 1; %i <= %clientId.bulkNum; %i++)
		{
			if(fetchData(%clientId, "SPcredits") > 0)
			{
				if(AddSkillPoint(%clientId, %o))
					storeData(%clientId, "SPcredits", 1, "dec");
				else
					break;
			}
			else
				break;
		}

		RefreshAll(%clientId);
	}

	if(%o != "done")
		MenuSP(%clientId, %p);
	else Game::menuRequest(%clientId);
}
function processMenunull(%clientId, %opt)
{
	return;
}

function MenuGroup(%clientId)
{
	dbecho($dbechoMode, "MenuGroup(" @ %clientId @ ")");

	Client::buildMenu(%clientId, "Pick a group:", "pickgroup", true);
	Client::addMenuItem(%clientId, "1Priest", 1);
	Client::addMenuItem(%clientId, "2Rogue", 2);
	Client::addMenuItem(%clientId, "3Warrior", 3);
	Client::addMenuItem(%clientId, "4Wizard", 4);

	return;
}
function processMenupickgroup(%clientId, %opt)
{
	dbecho($dbechoMode, "processMenupickgroup(" @ %clientId @ ", " @ %opt @ ")");

	if(%opt == 1)
		storeData(%clientId, "GROUP", "Priest");
	else if(%opt == 2)
		storeData(%clientId, "GROUP", "Rogue");
	else if(%opt == 3)
		storeData(%clientId, "GROUP", "Warrior");
	else if(%opt == 4)
		storeData(%clientId, "GROUP", "Wizard");
	else{
		MenuGroup(%clientId);
		return;
	}

	%clientId.choosingGroup = "";
	%clientId.choosingClass = True;

	MenuClass(%clientId);
}

function MenuClass(%clientId)
{
	dbecho($dbechoMode, "MenuClass(" @ %clientId @ ")");

	Client::buildMenu(%clientId, "Pick a class:", "pickclass", true);

	%op = 0;
	for(%i = 1; $ClassName[%i] != ""; %i++)
	{
		if(String::ICompare(fetchData(%clientId, "GROUP"), $ClassGroup[$ClassName[%i]]) == 0)
		{
			%op++;
			Client::addMenuItem(%clientId, %op @ $ClassName[%i], %op);
		}
	}
	Client::addMenuItem(%clientId, "x<-- BACK", "back");


	return;
}
function processMenupickclass(%clientId, %opt)
{
	dbecho($dbechoMode, "processMenupickclass(" @ %clientId @ ", " @ %opt @ ")");

	if(%opt == "back")
	{
		%clientId.choosingClass = "";
		%clientId.choosingGroup = True;
		storeData(%clientId, "GROUP", "");

		MenuGroup(%clientId);
		return;
	}

	%op = 0;
	for(%i = 1; $ClassName[%i] != ""; %i++)
	{
		if(String::ICompare(fetchData(%clientId, "GROUP"), $ClassGroup[$ClassName[%i]]) == 0)
		{
			%op++;
			if(%op == %opt){
				storeData(%clientId, "CLASS", $ClassName[%i]);
				%className = $ClassName[%i];
			}
		}
	}
	
	
	%giveNewcomerStuff = "";
	if(%className == "")
	{
		%clientId.choosingClass = "";
		%clientId.choosingGroup = True;
		storeData(%clientId, "GROUP", "");

		MenuGroup(%clientId);
		return;
	} else {
		%giveNewcomerStuff = "BeltItemTool 1";
		if($SkillMultiplier[%className, $SkillPiercing] >= 1.0)		%giveNewcomerStuff = %giveNewcomerStuff @ " Knife 1";
		if($SkillMultiplier[%className, $SkillBludgeoning] >= 1.0)	%giveNewcomerStuff = %giveNewcomerStuff @ " BoneClub 1";
		if($SkillMultiplier[%className, $SkillSlashing] >= 1.0)		%giveNewcomerStuff = %giveNewcomerStuff @ " Hatchet 1";
		if($SkillMultiplier[%className, $SkillWands] >= 1.0)		%giveNewcomerStuff = %giveNewcomerStuff @ " CastingBlade 1";
		if($SkillMultiplier[%className, $SkillArchery] >= 1.0)		%giveNewcomerStuff = %giveNewcomerStuff @ " Sling 1 Granite 20";
		
		if($SkillMultiplier[%className, $SkillWands] < 1 &&
		$SkillMultiplier[%className, $SkillArchery] < 1 && 
		$SkillMultiplier[%className, $SkillSlashing] < 1 && 
		$SkillMultiplier[%className, $SkillPiercing] < 1 && 
		$SkillMultiplier[%className, $SkillBludgeoning]< 1 ) {
			%giveNewcomerStuff = %giveNewcomerStuff @ " Knife 1";
		}
		
		if($SkillMultiplier[%className, $SkillMining] >= 1.0)		%giveNewcomerStuff = %giveNewcomerStuff @ " Diamond 1";
		if($SkillMultiplier[%className, $SkillWordsmith] >= 1.0)	%giveNewcomerStuff = %giveNewcomerStuff @ " Quartz 20 Silver 5 Parchment 10";
		if($SkillMultiplier[%className, $SkillEnergy] >= 1.0)		%giveNewcomerStuff = %giveNewcomerStuff @ " EnergyScroll 2 EnergyVial 1";
		if($SkillMultiplier[%className, $SkillSurvival] >= 1.0)		%giveNewcomerStuff = %giveNewcomerStuff @ " WaterFlask 2 Bandages 10";
		if($SkillMultiplier[%className, $SkillVitality] >= 1.0)		%giveNewcomerStuff = %giveNewcomerStuff @ " HealingHerbs 8 DriedBerries 8";
		if($SkillMultiplier[%className, $SkillThievery] >= 1.0)		%giveNewcomerStuff = %giveNewcomerStuff @ " BlackMarketKey 1";
	}
		

	//let the player enter the world
	%clientId.choosingClass = "";
	storeData(%clientId, "spawnStuff", %giveNewcomerStuff);
	Game::playerSpawn(%clientId, false);
	storeData(%clientId, "spawnStuff", "");
	//storeData(%clientId, "spawnStuff", %giveNewcomerStuff);

	//######### set a few start-up variables ########
	storeData(%clientId, "COINS", GetRoll($initcoins[fetchData(%clientId, "GROUP")]));

	//add $autoStartupSP for each skill
	for(%i = 1; %i <= getNumSkills(); %i++)
		AddSkillPoint(%clientId, %i, $autoStartupSP);

	centerprint(%clientId, 	"<f1>TribesRPG: World Ender\n" @ 
							"<f2>https://tribesrpg.org/worldender/\n" @ 
							"<f1>Server Admin: <f0>DescX\n" @
							"<f0>Need <f2>HELP?<f0> Press <f2>TAB!<f0>\n" @
							"Picked the wrong class? Send chat: <f2>#resetcharacter " @ rpg::getName(%clientId) @ "\n\n<f0>" @ 
							$loginMsg, 20);
	schedule("ProcessMenuhelp(" @ %clientId @ ", \"tips\", 1);", 20);
	schedule("ProcessMenuhelp(" @ %clientId @ ", \"wheretogo\", 1);", 40);
}

function OldGetLevel(%ex, %clientId)
{
	dbecho($dbechoMode, "GetLevel(" @ %ex @ ", " @ %clientId @ ")");

	%m = GetEXPmultiplier(%clientId);

	if(%m != 0)
	{
		%a = (  (-500 * %m) + RoundToFirstDecimal(sqrt( (250000 * %m * %m) + (2000 * %m * %ex) ))  ) / (1000 * %m);
		%b = floor(%a) + 1;
	}

	return %b;
}
function OldGetExp(%level, %clientId)
{
	dbecho($dbechoMode, "GetExp(" @ %level @ ", " @ %clientId @ ")");

	%m = GetEXPmultiplier(%clientId);

	%level--;
	%a = (500 * %level) + (500 * %level * %level);
	%b = floor( (%a * %m) + 0.2);

	return %b;
}

function GetLevel(%ex, %clientId)
{
	dbecho($dbechoMode, "GetLevel(" @ %ex @ ", " @ %clientId @ ")");

	%n = 1000;
	%b = floor(%ex / %n) + 1;

	return %b;
}
function GetExp(%level, %clientId)
{
	dbecho($dbechoMode, "GetExp(" @ %level @ ", " @ %clientId @ ")");

	%n = 1000;
	%b = (%level - 1) * %n;

	return %b;
}

$RPGXPMultiplier = "2.0";

function DistributeExpForKilling(%damagedClient)
{
	dbecho($dbechoMode2, "DistributeExpForKilling(" @ %damagedClient @ ")");

	%dname = Client::getName(%damagedClient);
	%dlvl = fetchData(%damagedClient, "LVL");

	%count = 0;

	//parse $damagedBy and create %finalDamagedBy
	%nameCount = 0;
	%listCount = 0;
	%total = 0;
	for(%i = 1; %i <= $maxDamagedBy; %i++)
	{
		if($damagedBy[%dname, %i] != "")
		{
			%listCount++;

			%n = GetWord($damagedBy[%dname, %i], 0);
			%d = GetWord($damagedBy[%dname, %i], 1);

			%flag = 0;
			for(%z = 1; %z <= %nameCount; %z++)
			{
				if(%finalDamagedBy[%z] == %n)
				{
					%flag = 1;
					%dCounter[%n] += %d;
				}
			}
			if(%flag == 0)
			{
				%nameCount++;
				%finalDamagedBy[%nameCount] = %n;
				%dCounter[%n] = %d;

				%p = IsInWhichParty(%n);
				if(%p != -1)
				{
					%id = GetWord(%p, 0);
					%inv = GetWord(%p, 1);
					if(%inv == -1)
					{
						%tmppartylist[%id] = %tmppartylist[%id] @ %n @ " ";
						if(String::findSubStr(%tmpl, %id @ " ") == -1)
							%tmpl = %tmpl @ %id @ " ";
					}
				}
			}
			%total += %d;
		}
	}

	//clear $damagedBy
	for(%i = 1; %i <= $maxDamagedBy; %i++)
		$damagedBy[%dname, %i] = "";

	//parse thru all tmppartylists and determine the number of same party members involved in exp split
	for(%w = 0; (%a = GetWord(%tmpl, %w)) != -1; %w++)
	{
		%n = CountObjInList(%tmppartylist[%a]);
		for(%ww = 0; (%aa = GetWord(%tmppartylist[%a], %ww)) != -1; %ww++)
			%partyFactor[%aa] = %n;
	}

	//distribute exp
	for(%i = 1; %i <= %nameCount; %i++)
	{
		if(%finalDamagedBy[%i] != "")
		{
			%listClientId = NEWgetClientByName(%finalDamagedBy[%i]);

			%slvl = fetchData(%listClientId, "LVL");

			if(RPG::isAiControlled(%damagedClient))
			{
				if(%slvl > 100)
					%value = 0;
				else
				{
					%f = (101 - %slvl) / 10;
					if(%f < 1) %f = 1;

					%a = (%dlvl - %slvl) + 8;
					%b = %a * %f;
					if(%b < 1) %b = 1;

					%z = %b * 0.10;
					%y = getRandom() * %z;
					%r = %y - (%z / 2);

					%c = %b + %r;

					%value = %c;
				}
			}
			else
			{
				%value = 0;
			}

			//rank point bonus
			if(fetchData(%listClientId, "MyHouse") != "")
			{
				%ph = Cap(rpg::GetHouseLevel(%listClientId) / 30, 1.00, 3.00);
				%value = %value * %ph;
			}
			
			// DX
			%value = %value * $RPGXPMultiplier;

			%perc = %dCounter[%finalDamagedBy[%i]] / %total;
			%final = Cap(round( %value * %perc ), "inf", 1000);

			if(Player::getItemCount(%listClientId,SlayerGear0)) {
				%fragRank = round(Cap(%final / 9,1,"inf"));
				storeData(%listClientId, "RankPoints", %fragRank, "inc");
				if(fetchData(%listClientId, "RankPoints") >= 100001) {
					storeData(%listClientId, "RankPoints", 100000);
				} else {
					if(fetchData(%listClientId, "RankPoints") % 1000 == 0) {
						Client::sendMessage(%listClientId, $MsgGreen, "Your Slayer Gear has increased in level.");
					}
					else {
						Client::sendMessage(%listClientId, 0, "Your slaying earns you " @ %fragRank @ " rank points!" );
					}
				}
			}

			//determine party exp
			%pf = %partyFactor[%finalDamagedBy[%i]];
			if(%pf != "" && %pf >= 2)
				%pvalue = round(%final * (1.0 + (%pf * 0.1)));
			else
				%pvalue = 0;
			
			%expBuff = AddBonusStatePoints(%clientId, "EXP");
			%pvalue *= 1.0 + Cap(%expBuff/100,0,3);

			storeData(%listClientId, "EXP", %final, "inc");
			if(%final > 0)
				Client::sendMessage(%listClientId, 0, %dname @ " has died and you gained " @ %final @ " experience!");
			else if(%final < 0)
				Client::sendMessage(%listClientId, 0, %dname @ " has died and you lost " @ -%final @ " experience.");
			else if(%final == 0)
				Client::sendMessage(%listClientId, 0, %dname @ " has died.");

			if(%pvalue != 0)
			{
				storeData(%listClientId, "EXP", %pvalue, "inc");
				Client::sendMessage(%listClientId, $MsgWhite, "You have gained " @ %pvalue @ " party experience!");
			}

			Game::refreshClientScore(%listClientId);
		}
	}
}

function StartStatSelection(%clientId)
{
	dbecho($dbechoMode, "StartStatSelection(" @ %clientId @ ")");

	%group = nameToId("MissionGroup\\ObserverDropPoints");
	%observerMarker = Group::getObject(%group, 0);
	
	Client::setControlObject(%clientId, Client::getObserverCamera(%clientId));
	Observer::setFlyMode(%clientId, GameBase::getPosition(%observerMarker), GameBase::getRotation(%observerMarker), false, true);

	storeData(%clientId, "SPcredits", $initSPcredits);

	MenuGroup(%clientId);
}

function Game::refreshClientScore(%clientId)
{
	dbecho($dbechoMode2, "Game::refreshClientScore(" @ %clientId @ ")");

	if(fetchData(%clientId, "HasLoadedAndSpawned"))
	{
		if(GetLevel(fetchData(%clientId, "EXP"), %clientId) != fetchData(%clientId, "templvl") && fetchData(%clientId, "HasLoadedAndSpawned") && fetchData(%clientId, "templvl") != "")
		{
			//client has leveled up
			%lvls = (GetLevel(fetchData(%clientId, "EXP"), %clientId) - fetchData(%clientId, "templvl"));

			storeData(%clientId, "SPcredits", (%lvls * $SPgainedPerLevel), "inc");

			if(%lvls > 0)
			{
				if(%lvls == 1)
					Client::sendMessage(%clientId,0,"You have gained a level!");
				else
					Client::sendMessage(%clientId,0,"You have gained " @ %lvls @ " levels!");
				Client::sendMessage(%clientId,0,"Welcome to level " @ fetchData(%clientId, "LVL"));
				
				if(!Player::isAiControlled(%clientId) && fetchData(%clientId, "FAVOR") > 0) {
					Client::sendMessage(%clientId,$MsgGreen,"Your FAVOR is recognized. Your health and mana are restored.");
					setHP(%clientId,   fetchData(%clientId, "MaxHP"));
					setMANA(%clientId, fetchData(%clientId, "MaxMANA"));
				}
				PlaySound(SoundLevelUp, GameBase::getPosition(%clientId));
				
			}
			else if(%lvls < 0)
			{
				if(%lvls == -1)
					Client::sendMessage(%clientId,0,"You have lost a level...");		
				else
					Client::sendMessage(%clientId,0,"You have lost " @ -%lvls @ " levels...");
				Client::sendMessage(%clientId,0,"You are now level " @ fetchData(%clientId, "LVL"));
			}
		}
		storeData(%clientId, "templvl", GetLevel(fetchData(%clientId, "EXP"), %clientId));

		if(rpg::AllowRemort(%clientId, 125) >= 0) {	// Force remort at 125 levels + remort step * 10	
			storeData(%clientId, "currentlyRemorting", True);
			for(%i = 1; %i <= 20; %i++) {
				schedule("CreateAndDetBomb(" @ %clientId @ ", \"SpellFXflashlarge\", GameBase::getPosition(" @ %clientId @ "), False, 19);", %i * 3, %clientId);
			}
			schedule("DoRemort(" @ %clientId @ ");", 60, %clientId);
		}
	}

	%z = Zone::getDesc(fetchData(%clientId, "zone"));
	if(%z == -1)
		%z = "unknown";

	if($displayPingAndPL)
		Client::setScore(%clientId, "%n\t" @ %z @ "\t  " @ fetchData(%clientId, "LVL") @ "\t%p\t%l", fetchData(%clientId, "LVL"));
	else
	{
            Client::setScore(%clientId, "%n\t" @ %z @ "\t  " @ fetchData(%clientId, "LVL") @ "\t" @ getFinalCLASS(%clientId) @ "\t%l", fetchData(%clientId, "LVL"));
	}
}

function GetSkillWithBonus(%clientId, %skill) {
	return $PlayerSkill[%clientId, %skill] + AddBonusStatePoints(%clientId, GetWord($SkillDesc[%skill],0));
}

function DoRemort(%clientId)
{
	dbecho($dbechoMode, "DoRemort(" @ %clientId @ ")");

	storeData(%clientId, "RemortStep", 1, "inc");

	storeData(%clientId, "EXP", 0);
	storeData(%clientId, "templvl", 1);
	storeData(%clientId, "FAVOR", $initialFavor, "inc");
	storeData(%clientId, "SPcredits", $initSPcredits, "inc");
	storeData(%clientId, "currentlyRemorting", "");

	//skill variables
	%cnt = 0;
	for(%i = 1; %i <= GetNumSkills(); %i++)
	{
		$PlayerSkill[%clientId, %i] = 0;
		$SkillCounter[%clientId, %i] = 0;
	}
	for(%i = 1; %i <= getNumSkills(); %i++)
		AddSkillPoint(%clientId, %i, $autoStartupSP);

	UnequipMountedStuff(%clientId);
	
	Player::setDamageFlash(%clientId, 1.0);
	Item::setVelocity(%clientId, "0 0 0");
	%pos = TeleportToMarker(%clientId, "Teams/team0/DropPoints", 0, 0);

	playSound(RespawnC, GameBase::getPosition(%clientId));
	
	RefreshAll(%clientId);

	Client::sendMessage(%clientId, $MsgBeige, "Welcome to Remort Level " @ fetchData(%clientId, "RemortStep") @ "!");

	return %pos;
}



MineData SpellFXflashlarge
{
	mass = 0.3;
	drag = 1.0;
	density = 2.0;
	elasticity = 0.15;
	friction = 1.0;
	className = "Handgrenade";
	description = "Handgrenade";
	shapeFile = "smoke";
	shadowDetailMask = 4;
	explosionId = flashExpLarge;
	explosionRadius = 10.0;
	damageValue = 1.0;
	damageType = $NullDamageType;
	kickBackStrength = 0;
	triggerRadius = 0.5;
	maxDamage = 1.0;
};
function SpellFXflashlarge::onAdd(%this) { schedule("Mine::Detonate(" @ %this @ ");", 0.2, %this); }

