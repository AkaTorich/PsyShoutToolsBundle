[Setup]
AppId={{A180A1CE-68C2-4268-9F9E-78C29C72AAA5}}
AppName=PsyShoutProducerTools
AppVersion=7.0
AppPublisher=PsyShout Art
AppPublisherURL=https://psyshout.gumroad.com/
AppSupportURL=https://psyshout.gumroad.com/
AppUpdatesURL=https://psyshout.gumroad.com/
DefaultDirName={commonpf}\PsyShoutProducerTools
; Включаем страницу выбора директории установки
DisableDirPage=no
UninstallDisplayIcon={app}\PADGenerator.exe
DisableProgramGroupPage=yes
OutputBaseFilename=PsyShoutProducerBundle Signed
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
LicenseFile={#file "License_en.txt"}
; Добавляем условие перехода на страницу удаления предыдущей версии
UsePreviousAppDir=yes
; Всегда создавать новый файл журнала удаления
UninstallRestartComputer=no
; Создавать журнал удаления
UninstallLogMode=append

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"; LicenseFile: "{#file 'License_en.txt'}"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"; LicenseFile: "{#file 'License_ru.txt'}"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\PADGenerator.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\ARPGenerator.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\BASSGenerator.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\ScaleSelector.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\BpmKeyDetector.exe"; DestDir: "{app}"; Flags: ignoreversion

; Сертификаты - добавлен флаг deleteafterinstall
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\PsyShout.pfx"; DestDir: "{app}"; Flags: ignoreversion deleteafterinstall
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\PsyShout.cer"; DestDir: "{app}"; Flags: ignoreversion deleteafterinstall

; Необходимые зависимости
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\NAudio.Asio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\NAudio.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\NAudio.Wasapi.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\NAudio.Midi.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\NAudio.WinForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\NAudio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\NAudio.Lame.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\defaultuser0\Desktop\PsyShoutTools\bin\Release\NAudio.WinMM.dll"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\PsyShoutProducerTools\PADGenerator"; Filename: "{app}\PADGenerator.exe"
Name: "{autoprograms}\PsyShoutProducerTools\ARPGenerator"; Filename: "{app}\ARPGenerator.exe"
Name: "{autoprograms}\PsyShoutProducerTools\BASSGenerator"; Filename: "{app}\BASSGenerator.exe"
Name: "{autoprograms}\PsyShoutProducerTools\ScaleSelector"; Filename: "{app}\ScaleSelector.exe"
Name: "{autoprograms}\PsyShoutProducerTools\BpmKeyDetector"; Filename: "{app}\BpmKeyDetector.exe"
; Создание ярлыков на рабочем столе для всех программ при выборе соответствующей опции
Name: "{autodesktop}\PADGenerator"; Filename: "{app}\PADGenerator.exe"; Tasks: desktopicon
Name: "{autodesktop}\ARPGenerator"; Filename: "{app}\ARPGenerator.exe"; Tasks: desktopicon
Name: "{autodesktop}\BASSGenerator"; Filename: "{app}\BASSGenerator.exe"; Tasks: desktopicon
Name: "{autodesktop}\ScaleSelector"; Filename: "{app}\ScaleSelector.exe"; Tasks: desktopicon
Name: "{autodesktop}\BpmKeyDetector"; Filename: "{app}\BpmKeyDetector.exe"; Tasks: desktopicon

[Run]
; Импорт сертификата в личное хранилище с использованием пароля
Filename: "certutil.exe"; Parameters: "-p ""1z48Smd8"" -importPFX ""My"" ""{app}\PsyShout.pfx"""; WorkingDir: "{app}"; Flags: runhidden waituntilterminated; StatusMsg: "Импорт сертификата в личное хранилище"
; Добавление сертификата в корневое хранилище
Filename: "certutil.exe"; Parameters: "-addstore ""Root"" ""{app}\PsyShout.cer"""; WorkingDir: "{app}"; Flags: runhidden waituntilterminated; StatusMsg: "Добавление сертификата в корневое хранилище"
; Принудительное удаление файлов сертификатов
Filename: "cmd.exe"; Parameters: "/c del ""{app}\PsyShout.pfx"" ""{app}\PsyShout.cer"""; WorkingDir: "{app}"; Flags: runhidden waituntilterminated; StatusMsg: "Удаление файлов сертификатов"

; Опциональный запуск PADGenerator после установки
Filename: "{app}\PADGenerator.exe"; Description: "Запустить PADGenerator"; Flags: nowait postinstall skipifsilent unchecked

[InstallDelete]
; Удаление всех файлов из папки назначения перед установкой
Type: filesandordirs; Name: "{app}\*"

[Code]
// Функция для удаления предыдущей установки
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  UninstallString: String;
  UninstallArgs: String;
  ResultCode: Integer;
begin
  Result := '';
  
  // Проверяем, есть ли уже установленная версия
  if RegQueryStringValue(HKLM, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{A180A1CE-68C2-4268-9F9E-78C29C72AAA5}_is1', 'UninstallString', UninstallString) then
  begin
    // Убедимся, что путь в кавычках
    if Pos('"', UninstallString) = 1 then
    begin
      // Находим второй апостроф и извлекаем путь
      UninstallArgs := ' /SILENT /SUPPRESSMSGBOXES /NORESTART';
    end
    else 
    begin
      // Добавляем кавычки
      UninstallString := '"' + UninstallString + '"';
      UninstallArgs := ' /SILENT /SUPPRESSMSGBOXES /NORESTART';
    end;
    
    // Выполняем тихую деинсталляцию
    if not Exec(UninstallString, UninstallArgs, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    begin
      Result := 'Ошибка при удалении предыдущей версии. Код: ' + IntToStr(ResultCode);
    end;
    
    // Даем системе время на освобождение файлов
    Sleep(1000);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Дополнительное удаление сертификатов для большей надежности
    DeleteFile(ExpandConstant('{app}\PsyShout.pfx'));
    DeleteFile(ExpandConstant('{app}\PsyShout.cer'));
    
    MsgBox('Установка завершена.', mbInformation, MB_OK);
  end;
end;

// Функция для очистки директории при удалении
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Удаление любых оставшихся файлов и директории после деинсталляции
    DelTree(ExpandConstant('{app}'), True, True, True);
  end;
end;