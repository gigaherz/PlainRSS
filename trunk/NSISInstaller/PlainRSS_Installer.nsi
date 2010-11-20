; Script generated with the Venis Install Wizard

; Define your application name
!define APPNAME "PlainRSS"
!define APPNAMEANDVERSION "PlainRSS 1.1.0.20"

; Main Install settings
Name "${APPNAMEANDVERSION}"
InstallDir "$PROGRAMFILES\PlainRSS"
InstallDirRegKey HKLM "Software\${APPNAME}" ""
OutFile ".\PlainRSS_Installer.exe"

; Modern interface settings
!include "MUI.nsh"

!define MUI_ABORTWARNING
!define MUI_FINISHPAGE_RUN "$INSTDIR\PlainRSS.exe"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

; Set languages (first is default language)
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_RESERVEFILE_LANGDLL

Section "PlainRSS" Section1

	; Set Section properties
	SetOverwrite on

	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\"
	File "..\PlainRSS\bin\Release\Atom.NET.dll"
	File "..\PlainRSS\bin\Release\PlainRSS.exe"
	File "..\PlainRSS\bin\Release\RSS.NET.dll"
	CreateDirectory "$INSTDIR\Resources"
	SetOutPath "$INSTDIR\Resources"
	File "..\PlainRSS\bin\Release\Resources\feed-icon-14x14.png"
	CreateShortCut "$DESKTOP\PlainRSS.lnk" "$INSTDIR\PlainRSS.exe"
	CreateDirectory "$SMPROGRAMS\PlainRSS"
	CreateShortCut "$SMPROGRAMS\PlainRSS\PlainRSS.lnk" "$INSTDIR\PlainRSS.exe"
	CreateShortCut "$SMPROGRAMS\PlainRSS\Uninstall.lnk" "$INSTDIR\uninstall.exe"

SectionEnd

Section -FinishSection

	WriteRegStr HKLM "Software\${APPNAME}" "" "$INSTDIR"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$INSTDIR\uninstall.exe"
	WriteUninstaller "$INSTDIR\uninstall.exe"

SectionEnd

; Modern install component descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
	!insertmacro MUI_DESCRIPTION_TEXT ${Section1} ""
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;Uninstall section
Section Uninstall

	;Remove from registry...
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
	DeleteRegKey HKLM "SOFTWARE\${APPNAME}"

	; Delete self
	Delete "$INSTDIR\uninstall.exe"

	; Delete Shortcuts
	Delete "$DESKTOP\PlainRSS.lnk"
	Delete "$SMPROGRAMS\PlainRSS\PlainRSS.lnk"
	Delete "$SMPROGRAMS\PlainRSS\Uninstall.lnk"

	; Clean up PlainRSS
	Delete "$INSTDIR\Atom.NET.dll"
	Delete "$INSTDIR\PlainRSS.exe"
	Delete "$INSTDIR\RSS.NET.dll"
	Delete "$INSTDIR\Resources\feed-icon-14x14.png"

	; Remove remaining directories
	RMDir "$SMPROGRAMS\PlainRSS"
	RMDir "$INSTDIR\Resources"
	RMDir "$INSTDIR\"

SectionEnd

; eof