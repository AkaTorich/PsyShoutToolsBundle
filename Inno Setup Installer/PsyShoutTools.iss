[Setup]
AppId={{A180A1CE-68C2-4268-9F9E-78C29C72AAA5}}
AppName=PsyShoutTools
AppVersion=6.7
AppPublisher=PsyShout Art
AppPublisherURL=https://psyshout.gumroad.com/
AppSupportURL=https://psyshout.gumroad.com/
AppUpdatesURL=https://psyshout.gumroad.com/
DefaultDirName=C:\ProgramData\PsyShoutTools
DisableDirPage=yes
UninstallDisplayIcon={app}\PsyShoutLauncher.exe
DisableProgramGroupPage=yes
OutputBaseFilename=PsyShoutTools Installer Signed
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
LicenseFile={#file "License_en.txt"}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"; LicenseFile: "{#file 'License_en.txt'}"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"; LicenseFile: "{#file 'License_ru.txt'}"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\PsyShoutLauncher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\ARPGenerator.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\BASSGenerator.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\BpmKeyDetector.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\AudioConverter.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\AudioConverter.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\AudioCutter.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\BackupManager.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\AudioCutter.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\DreamDataManager.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\ImageResizer.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\LayoutConverter.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\libmp3lame.32.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\libmp3lame.64.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\MediciGet.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\NAudio.Asio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\NAudio.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\NAudio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\NAudio.Lame.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\NAudio.Midi.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\NAudio.Wasapi.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\NAudio.WinForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\NAudio.WinMM.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\PADGenerator.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\PomedoroWorker.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\ProjectManager.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\ScaleSelector.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\TaskMan.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\IChing.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\BrainWaveGenerator.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\AmAwake.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\PlayOnMe.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\TasksReminder.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\Archiver.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\BouncyCastle.Cryptography.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\MailKit.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\MimeKit.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\System.Formats.Asn1.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\System.Numerics.Vectors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\TagLibSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\WavTagEditor.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\MailClient.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\NoteTray.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\NetMonitor.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\PsyShout.pfx"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\PsyShout.cer"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\bass.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\break.wav"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\work.wav"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\portaudio_x64.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\portaudio_x86.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\MAC.db"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Admin\Desktop\PsyShoutTools\bin\Release\MailClient.exe.config"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\PsyShoutTools"; Filename: "{app}\PsyShoutLauncher.exe"
Name: "{autodesktop}\PsyShoutTools"; Filename: "{app}\PsyShoutLauncher.exe"; Tasks: desktopicon

[Run]
Filename: "certutil.exe"; Parameters: "-p ""1z48Smd8"" -importPFX ""My"" ""{app}\PsyShout.pfx"""; WorkingDir: "{app}"; Flags: runhidden waituntilterminated; StatusMsg: "Importing certificate to Personal store"
Filename: "certutil.exe"; Parameters: "-addstore ""Root"" ""{app}\PsyShout.cer"""; WorkingDir: "{app}"; Flags: runhidden waituntilterminated; StatusMsg: "Adding certificate to Root store"
Filename: "{sys}\cmd.exe"; Parameters: "/c del ""{app}\PsyShout.pfx"" ""{app}\PsyShout.cer"""; WorkingDir: "{app}"; Flags: runhidden waituntilterminated; StatusMsg: "Удаление файлов сертификатов"
Filename: "{app}\PsyShoutLauncher.exe"; Description: "{cm:LaunchProgram,PsyShoutTools}"; Flags: nowait postinstall skipifsilent

[Code]
// Функция для удаления всех файлов из директории (без рекурсии)
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

// Функция для удаления директорий (рекурсивно)
procedure DeleteDirsFromDir(const DirName: string);
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
        if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY <> 0) then
        begin
          // Рекурсивно удаляем файлы из поддиректорий
          DeleteFilesFromDir(FilePath);
          // Рекурсивно удаляем поддиректории из поддиректорий
          DeleteDirsFromDir(FilePath);
          // Пытаемся удалить директорию и игнорируем ошибки
          if not RemoveDir(FilePath) then
            Log('Не удалось удалить директорию: ' + FilePath);
        end;
      end;
    until not FindNext(FindRec);
  finally
    FindClose(FindRec);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  CertPfxPath, CertCerPath: string;
begin
  if CurStep = ssInstall then
  begin
    // Очистка директории установки перед установкой новых файлов
    if DirExists(ExpandConstant('{app}')) then
    begin
      Log('Очистка директории установки: ' + ExpandConstant('{app}'));
      
      // Сначала удаляем все файлы
      DeleteFilesFromDir(ExpandConstant('{app}'));
      
      // Затем удаляем все поддиректории
      DeleteDirsFromDir(ExpandConstant('{app}'));
    end;
  end
  else if CurStep = ssPostInstall then
  begin
    // Удаляем файлы сертификатов после импорта
    CertPfxPath := ExpandConstant('{app}\PsyShout.pfx');
    CertCerPath := ExpandConstant('{app}\PsyShout.cer');
    
    if FileExists(CertPfxPath) then
    begin
      Log('Удаление файла сертификата PFX: ' + CertPfxPath);
      if not DeleteFile(CertPfxPath) then
        Log('Не удалось удалить файл сертификата PFX: ' + CertPfxPath);
    end;
    
    if FileExists(CertCerPath) then
    begin
      Log('Удаление файла сертификата CER: ' + CertCerPath);
      if not DeleteFile(CertCerPath) then
        Log('Не удалось удалить файл сертификата CER: ' + CertCerPath);
    end;
    
    MsgBox('Установка завершена.', mbInformation, MB_OK);
  end;
end;