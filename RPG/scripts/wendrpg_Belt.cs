//This file is part of Tribes RPG.
function belt::checkmenus(%clientId)





	for(%i = %lb; %i <= %ub; %i++)


	if(%type == "AmmoItems"){

	if(!($ItemDropFlags[%item] & $ItemDropNever)){
		Client::addMenuItem(%clientId, %cnt++ @ "Drop "@%amnt, %type@" drop "@%item@" "@%amnt);
}

	else if(%option == "arm")
			%mp = fetchData(%clientId, "MANA");
			if(fetchData(%clientId, "MP") != %mp)
				UseSkill(%clientId, $SkillEnergy, True, True);



function MenuSellBelt(%clientId, %page)

function processMenuSellBelt(%clientId, %opt)






function MenuSellBeltItemFinal(%clientid, %item, %type)




function processMenuBuyBeltItemFinal(%clientId, %opt)

function processMenuSellBeltItemFinal(%clientId, %opt)

//-----------------------------------------------------------------
function MenuStoreBelt(%clientId, %page)
}

function processMenuStoreBelt(%clientId, %opt)
function belt::checkbankmenus(%clientId)
function belt::buildBankMenu(%clientId, %page){
function MenuWithdrawBelt(%clientId, %page)
function processMenuWithdrawBelt(%clientId, %opt)
function MenuStoreBeltItem(%clientid, %type, %page)
function processMenuStoreBeltItem(%clientid, %opt)
function MenuStoreBeltItemFinal(%clientid, %item, %type) {	


//---------------------------
function MenuWithdrawBeltItemFinal(%clientid, %item, %type)


	%list[4] = "GlassIdioms";
	
function BeltItem::Add(%name,%item,%type,%weight,%cost,%miscInfo,%specialDropFlags) {

function Belt::TakeThisStuff(%clientid,%item,%amnt)
function isBeltItem(%item)
function Belt::AddToList(%list, %item)
function Belt::BankGiveThisStuff(%clientid, %item, %amnt)
function Belt::BankHasThisStuff(%clientid,%item)

	