#define PACKAGE "cave-tools"
#define NAME "CaveSystems Tools"
#define COMPANY "CaveSystems GmbH"
#define VERSION "1.0.4"

[Setup]
AppId={#PACKAGE}
AppName={#NAME}
AppVersion={#VERSION}
AppPublisher={#COMPANY}
AppPublisherURL=http://www.cavesystems.de
AppSupportURL=http://www.caveprojects.org
AppUpdatesURL=http://www.caveprojects.org/downloads/cave-tools/
AppMutex=cave-tools
DefaultDirName={pf}\{#COMPANY}\Tools
DefaultGroupName={#NAME}
AllowNoIcons=yes
LicenseFile=license.rtf
OutputDir=.\bin
OutputBaseFileName=setup-{#PACKAGE}-v{#VERSION}
SetupIconFile=setup.ico
Compression=lzma2
SolidCompression=yes
AllowUNCPath=no
;DisableDirPage=yes
DisableReadyMemo=yes
DisableReadyPage=yes
ArchitecturesInstallIn64BitMode=x64
;Tools->Configure Sign Tools->add->default->cssign $p $f
;SignTool=default sign /v /i "COMODO" /n "CaveSystems GmbH" /a /t http://timestamp.verisign.com/scripts/timstamp.dll
UninstallDisplayIcon={app}\setup.ico
ChangesEnvironment=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "catalan"; MessagesFile: "compiler:Languages\Catalan.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "danish"; MessagesFile: "compiler:Languages\Danish.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "finnish"; MessagesFile: "compiler:Languages\Finnish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "hebrew"; MessagesFile: "compiler:Languages\Hebrew.isl"
Name: "hungarian"; MessagesFile: "compiler:Languages\Hungarian.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "norwegian"; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "slovenian"; MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[CustomMessages]

[Components]

[Files]
Source: "setup.ico"; DestDir: "{app}"; Flags: replacesameversion
Source: "license.rtf"; DestDir: "{app}"; Flags: replacesameversion
Source: "changes.txt"; DestDir: "{app}"; Flags: replacesameversion

Source: "Obfuscar.Console.exe"; DestDir: "{app}"; Flags: replacesameversion

Source: "bin\Debug\net47\*.*"; DestDir: "{app}"; Flags: replacesameversion

[Icons]
;Name: "{group}\{cm:UninstallProgram,{#NAME} {#VERSION}}"; Filename: "{uninstallexe}"
;Name: "{group}\{cm:ProgramOnTheWeb,{#COMPANY}}"; Filename: "http://www.cavesystems.de"

[Code]
function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
  sUnInstPath6432: String;
  sPackage: String;
begin
  sPackage := '{#PACKAGE}_is1'
  sUnInstPath := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\' + sPackage;
  sUnInstPath6432 := 'Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\' + sPackage;
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
  if not RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString) then
  if not RegQueryStringValue(HKLM, sUnInstPath6432, 'UninstallString', sUnInstallString) then
         RegQueryStringValue(HKCU, sUnInstPath6432, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
  Result := 0;
  sUnInstallString := RemoveQuotes(GetUninstallString());
  Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_SHOW, ewWaitUntilTerminated, iResultCode);
  if ('' <> RemoveQuotes(GetUninstallString())) then MsgBox('Could not uninstall old version!' + #10 + 'Please uninstall it manually and press ok afterwards!', mbError, MB_OK);
end;

const WM_SETTINGCHANGE = $001A;

procedure ModifyEnvironmentPath();
var
  currentPath: String;
  pathArray: TArrayOfString;
  pathCount: Integer;
  pathItem: String;
  newPathItem: String;
  i: Integer;	
begin
  pathCount:= 0;
  newPathItem:= ExpandConstant('{app}');

	//get current path
	RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', currentPath);

	//repeat until the whole path was parsed
	while (Length(currentPath) > 0) do
	begin
	  //find end of path item
	  i:= Pos(';', currentPath) - 1;
	  //found splitter ? no -> remainder is last path item
	  if (i < 0) then
    begin
      pathItem:= currentPath;
      currentPath:= '';
    end else begin
      pathItem:= Copy(currentPath, 0, i);
      currentPath:= Copy(currentPath, i + 2, Length(currentPath) - i - 1);
    end;
    
    //path item length > 0
	  if (Length(pathItem) > 0) then
	  begin
      //if the pathitem is not our app path add it to the list
      if (pathItem <> newPathItem) then
      begin
        pathCount:= pathCount + 1;
        SetArrayLength(pathArray, pathCount);
        pathArray[pathCount - 1]:= pathItem;
	    end;
	  end;	
	end;
	
  //create new full path string
  currentPath:= '';
	for i:= 0 to pathCount - 1 do currentPath:= currentPath + pathArray[i] + ';';
	//if we are installing add the new path
	if (not IsUninstaller()) then currentPath:= newPathItem + ';' + currentPath;
	//write the path
  RegWriteStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', currentPath);
  
  //StrCopy(l_NotifyString, 'Environment');
  //innosetup does not support pointers... this does not work
  //PostMessage(HWND_BROADCAST, WM_SETTINGCHANGE, 0, @'Environment'[1]);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssInstall) then UnInstallOldVersion();
  if (CurStep = ssPostInstall) then ModifyEnvironmentPath();
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
	if (CurUninstallStep = usUninstall) then ModifyEnvironmentPath();
end;

