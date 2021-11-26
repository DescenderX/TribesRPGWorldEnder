//_________________________________________________________________________________________________________________________________
// DescX Notes:
//		Not strictly necessary, but needed to let the server prompt the client to set a skiing key
//		Currently abusing the fact that rpg.cs is unused by WorldEnder to deliver
//		script-only updates that sync with client and server
function remoteForceJump(%clientId) {
	if(%clientId != 2048) return;
	postAction(%clientId, IDACTION_MOVEUP, -0);	
	if($LocalClient::Skiing == true)
		schedule("remoteForceJump("@ %clientId @ ");", 0.01);
}
function remoteForceJumpEnd(%clientId) {
	if(%clientId != 2048) return;
	$LocalClient::Skiing = false;
}
function remoteSetClientSkiButton(%clientId, %keyName, %on) {
	if(%clientId != 2048) return;	
	editActionMap("playMap.sae");	
	%whichDevice = "keyboard0";
	if(String::findSubStr(%keyName, "button") == 0)
		%whichDevice = "mouse0";	
	if(%on) {
		bindCommand(%whichDevice, make, %keyName, TO, "$LocalClient::Skiing = true; remoteForceJump(2048);");
		bindCommand(%whichDevice, break, %keyName, TO, "remoteForceJumpEnd(2048);");
	} else {
		bindAction(%whichDevice, make, %keyName, TO, IDACTION_MOVEUP);
		bindCommand(%whichDevice, break, %keyName, TO, "");
	}
}
function remotePlayAnimWav(%cl, %anim, %wav) {
	if(string::getSubStr(%wav, 0, 50) != %wav) return false;
	%anim = floor(%anim);
	remotePlayAnim(%cl, %anim);
	playVoice(%cl, %wav);
}
function remoteLMSG(%cl, %wav) {
	if(string::getSubStr(%wav, 0, 50) != %wav) return false;
	playVoice(%cl, %wav);
}
