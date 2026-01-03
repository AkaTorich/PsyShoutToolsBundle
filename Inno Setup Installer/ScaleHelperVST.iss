[Setup]
AppId={{B281C2DE-79D3-4369-8E0F-89D40E83BBB6}}
AppName=ScaleHelperVST
AppVersion=1.0
AppPublisher=PsyShout Arts
AppPublisherURL=https://yourwebsite.com/
AppSupportURL=https://yourwebsite.com/
AppUpdatesURL=https://yourwebsite.com/
DefaultDirName=C:\Program Files\VSTPlugins\ScaleHelperVST
DisableDirPage=no
UninstallDisplayIcon={app}\ScaleHelperVST.dll
DisableProgramGroupPage=yes
OutputBaseFilename=ScaleHelperVST_Installer
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Files]
; Основные файлы VST плагина
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\ScaleHelperVST\bin\Release\Jacobi.Vst.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\ScaleHelperVST\bin\Release\Jacobi.Vst.Framework.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\ScaleHelperVST\bin\Release\ScaleHelperVST.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\ScaleHelperVST\bin\Release\ScaleHelperVST.net.dll"; DestDir: "{app}"; Flags: ignoreversion

[UninstallDelete]
Type: files; Name: "{app}\*"
Type: dirifempty; Name: "{app}"

[Code]
// Функция для удаления всех файлов из директории
procedure DeleteFilesFromDir(const DirName: string);
var
  FindRec: TFindRec;
  FilePath: string;
begin
  if FindFirst(DirName + '\*', FindRec) then
  try
    repeat
      if (FindRec.Name <> '.') and (FindRec.Name <> '..') then
      begin
        FilePath := DirName + '\' + FindRec.Name;
        if not (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY <> 0) then
        begin
          // Пытаемся удалить файл и игнорируем ошибки
          if not DeleteFile(FilePath) then
            Log('Не удалось удалить файл: ' + FilePath);
        end;
      end;
    until not FindNext(FindRec);
  finally
    FindClose(FindRec);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    // Очистка директории установки перед установкой новых файлов
    if DirExists(ExpandConstant('{app}')) then
    begin
      Log('Очистка директории установки: ' + ExpandConstant('{app}'));
      DeleteFilesFromDir(ExpandConstant('{app}'));
    end;
  end
  else if CurStep = ssPostInstall then
  begin
    MsgBox('ScaleHelperVST успешно установлен в папку:' + #13#10 + 
           ExpandConstant('{app}') + #13#10 + #13#10 + 
           'Плагин готов к использованию в вашей DAW.', 
           mbInformation, MB_OK);
  end;
end;