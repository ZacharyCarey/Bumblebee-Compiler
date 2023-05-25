; --------------------------------------------------------------------------------------------------
;                            ntdsa.inc Copyright The MASM32 SDK 1998-2010
; --------------------------------------------------------------------------------------------------

IFNDEF NTDSA_INC
NTDSA_INC equ <1>

AppendRDN PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD
AttrTypeToKey PROTO STDCALL :DWORD,:DWORD
CountNameParts PROTO STDCALL :DWORD,:DWORD
CrackSingleName PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD
DSNAMEToHashKeyExternal PROTO STDCALL :DWORD
DSNAMEToMappedStrExternal PROTO STDCALL :DWORD
DSStrToHashKeyExternal PROTO STDCALL :DWORD,:DWORD
DSStrToMappedStrExternal PROTO STDCALL :DWORD,:DWORD
DbgPrintErrorInfo PROTO STDCALL
DirAddEntry PROTO STDCALL :DWORD,:DWORD
DirBind PROTO STDCALL :DWORD,:DWORD
DirCompare PROTO STDCALL :DWORD,:DWORD
DirErrorToNtStatus PROTO STDCALL :DWORD,:DWORD
DirErrorToWinError PROTO STDCALL :DWORD,:DWORD
DirFindEntry PROTO STDCALL :DWORD,:DWORD
DirGetDomainHandle PROTO STDCALL :DWORD
DirList PROTO STDCALL :DWORD,:DWORD
DirModifyDN PROTO STDCALL :DWORD,:DWORD
DirModifyEntry PROTO STDCALL :DWORD,:DWORD
DirNotifyRegister PROTO STDCALL :DWORD,:DWORD,:DWORD
DirNotifyUnRegister PROTO STDCALL :DWORD,:DWORD
DirOperationControl PROTO STDCALL :DWORD,:DWORD
DirPrepareForImpersonate PROTO STDCALL :DWORD,:DWORD,:DWORD
DirProtectEntry PROTO STDCALL :DWORD
DirRead PROTO STDCALL :DWORD,:DWORD
DirRemoveEntry PROTO STDCALL :DWORD,:DWORD
DirReplicaAdd PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD
DirReplicaDelete PROTO STDCALL :DWORD,:DWORD,:DWORD
DirReplicaDemote PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
DirReplicaGetDemoteTarget PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
DirReplicaModify PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD
DirReplicaReferenceUpdate PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
DirReplicaSetCredentials PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
DirReplicaSynchronize PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
DirSearch PROTO STDCALL :DWORD,:DWORD
DirStopImpersonating PROTO STDCALL :DWORD,:DWORD,:DWORD
DirTransactControl PROTO STDCALL :DWORD
DirUnBind PROTO STDCALL
DoLogEvent PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD
DoLogEventAndTrace PROTO STDCALL :DWORD
DoLogOverride PROTO STDCALL :DWORD,:DWORD
DsChangeBootOptions PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
DsCheckConstraint PROTO STDCALL :DWORD,:DWORD,:DWORD
DsFreeServersAndSitesForNetLogon PROTO STDCALL :DWORD
DsGetBootOptions PROTO STDCALL
DsGetDefaultObjCategory PROTO STDCALL :DWORD
DsGetEventConfig PROTO STDCALL
DsGetServersAndSitesForNetLogon PROTO STDCALL :DWORD,:DWORD
DsInitialize PROTO STDCALL :DWORD,:DWORD,:DWORD
DsInitializeCritSecs PROTO STDCALL
DsIsBeingBackSynced PROTO STDCALL
DsTraceEvent PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD
DsUninitialize PROTO STDCALL :DWORD
DsWaitUntilDelayedStartupIsDone PROTO STDCALL
DsaDisableUpdates PROTO STDCALL
DsaEnableUpdates PROTO STDCALL
DsaExeStartRoutine PROTO C :VARARG
DsaSetInstallCallback PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
DsaWaitUntilServiceIsRunning PROTO STDCALL :DWORD
FindNetbiosDomainName PROTO STDCALL :DWORD,:DWORD,:DWORD
GCVerifyCacheLookup PROTO STDCALL :DWORD
GetConfigDsName PROTO STDCALL :DWORD
GetConfigParam PROTO STDCALL :DWORD,:DWORD,:DWORD

GetConfigParamAllocW PROTO STDCALL :DWORD,:DWORD,:DWORD
IFDEF __UNICODE__
  GetConfigParamAlloc equ <GetConfigParamAllocW>
ENDIF

GetConfigParamW PROTO STDCALL :DWORD,:DWORD,:DWORD
IFDEF __UNICODE__
  GetConfigParam equ <GetConfigParamW>
ENDIF

GetConfigurationName PROTO STDCALL :DWORD,:DWORD,:DWORD
GetConfigurationNamesList PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
GetDnsRootAlias PROTO STDCALL :DWORD,:DWORD
GetRDNInfoExternal PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
GuidBasedDNSNameFromDSName PROTO STDCALL :DWORD
ImpersonateAnyClient PROTO STDCALL
InitCommarg PROTO STDCALL :DWORD
IsMangledRDNExternal PROTO STDCALL :DWORD,:DWORD,:DWORD
IsStringGuid PROTO STDCALL :DWORD,:DWORD
MapSpnServiceClass PROTO STDCALL :DWORD
MatchCrossRefByNetbiosName PROTO STDCALL :DWORD,:DWORD,:DWORD
MatchCrossRefBySid PROTO STDCALL :DWORD,:DWORD,:DWORD
MatchDomainDnByDnsName PROTO STDCALL :DWORD,:DWORD,:DWORD
MatchDomainDnByNetbiosName PROTO STDCALL :DWORD,:DWORD,:DWORD
MtxAddrFromTransportAddr PROTO STDCALL :DWORD
MtxSame PROTO STDCALL :DWORD,:DWORD
NameMatched PROTO STDCALL :DWORD,:DWORD
NameMatchedStringNameOnly PROTO STDCALL :DWORD,:DWORD
NamePrefix PROTO STDCALL :DWORD,:DWORD
QuoteRDNValue PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
SampAddLoopbackTask PROTO STDCALL :DWORD
SampAmIGC PROTO STDCALL
SampComputeGroupType PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
SampDeriveMostBasicDsClass PROTO STDCALL :DWORD
SampDoesDomainExist PROTO STDCALL :DWORD
SampDsAttrFromSamAttr PROTO STDCALL :DWORD,:DWORD
SampDsClassFromSamObjectType PROTO STDCALL :DWORD
SampExistsDsLoopback PROTO STDCALL :DWORD
SampExistsDsTransaction PROTO STDCALL
SampGCLookupNames PROTO STDCALL :DWORD,:DWORD,:DWORD
SampGCLookupSids PROTO STDCALL :DWORD,:DWORD,:DWORD
SampGetAccountCounts PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
SampGetClassAttribute PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
SampGetDisplayEnumerationIndex PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD
SampGetDsAttrIdByName PROTO STDCALL :DWORD,:DWORD
SampGetEnterpriseSidList PROTO STDCALL :DWORD,:DWORD
SampGetGroupsForToken PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
SampGetLoopbackObjectClassId PROTO STDCALL :DWORD
SampGetMemberships PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD
SampGetQDIRestart PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
SampGetSamAttrIdByName PROTO STDCALL :DWORD,:DWORD,:DWORD
SampGetServerRoleFromFSMO PROTO STDCALL :DWORD
SampIsSecureLdapConnection PROTO STDCALL
SampIsWriteLockHeldByDs PROTO STDCALL
SampMaybeBeginDsTransaction PROTO STDCALL :DWORD
SampMaybeEndDsTransaction PROTO STDCALL :DWORD
SampNetlogonPing PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD
SampSamAttrFromDsAttr PROTO STDCALL :DWORD,:DWORD
SampSamObjectTypeFromDsClass PROTO STDCALL :DWORD
SampSetDsa PROTO STDCALL :DWORD
SampSetIndexRanges PROTO STDCALL :DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD,:DWORD
SampSetLsa PROTO STDCALL :DWORD
SampSetSam PROTO STDCALL :DWORD
SampSignalStart PROTO STDCALL
SampVerifySids PROTO STDCALL :DWORD,:DWORD,:DWORD
THAlloc PROTO STDCALL :DWORD
THClearErrors PROTO STDCALL
THCreate PROTO STDCALL :DWORD
THDestroy PROTO STDCALL
THFree PROTO STDCALL :DWORD
THGetErrorString PROTO STDCALL
THQuery PROTO STDCALL
THReAlloc PROTO STDCALL :DWORD,:DWORD
THRestore PROTO STDCALL :DWORD
THSave PROTO STDCALL
THVerifyCount PROTO STDCALL :DWORD
TransportAddrFromMtxAddr PROTO STDCALL :DWORD
TrimDSNameBy PROTO STDCALL :DWORD,:DWORD,:DWORD
UnImpersonateAnyClient PROTO STDCALL
UpdateDSPerfStats PROTO STDCALL :DWORD,:DWORD,:DWORD
fNullUuid PROTO STDCALL :DWORD

ELSE
  echo ----------------------------------------
  echo WARNING duplicate include file ntdsa.inc
  echo ----------------------------------------
ENDIF