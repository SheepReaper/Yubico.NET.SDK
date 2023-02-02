---
uid: Fido2CredentialManagement
---

<!-- Copyright 2023 Yubico AB

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License. -->

# FIDO2 credential management

The credential management operations allow you to obtain information about the credentials
on a YubiKey without getting an assertion. Note that you can get information only for
discoverable credentials. Remember that to make a credential discoverable, when you make
it (see [MakeCredential](xref:Yubico.YubiKey.Fido2.Fido2Session.MakeCredential%2a)), set
the "`rk`" option to `true`

```csharp
    var makeCredentialParameters = new MakeCredentialParameters(relyingParty, userEntity)
    {
        ClientDataHash = clientDataHash,
    };
    makeCredParams.AddOption(AuthenticatorOptions.rk, true);
    MakeCredentialData credentialData = fido2Session.MakeCredential(makeCredentialParameters);
```

These are the credential management operations:

* [Get Metadata](#get-metadata)
* [Enumerate Relying Parties](#enumerate-relying-parties)
* [Enumerate Credentials](#enumerate-credentials)
* [Delete Credential](#delete-credential)
* [Update User Information](#update-user-information)

In the SDK, the operations that return data will return an instance of the
[CredentialManagementData](xref:Yubico.YubiKey.Fido2.CredentialManagementData) class. That
class contains properties for all possible return values.

* `NumberOfDiscoverableCredentials`
* `RemainingCredentialCount`
* `RelyingParty`
* `RelyingPartyIdHash`
* `TotalRelyingPartyCount`
* `User`
* `CredentialId`
* `CredentialPublicKey`
* `TotalCredentialsForRelyingParty`
* `CredProtectPolicy`
* `LargeBlobKey`

Each operation that does return data will return two or more of these elements. After
performing an operation, look at the resulting `CredentialManagementData` object. Some of
the properties will be null, that operation did not return that item, or not null, the
information requested.

## Support in the YubiKey

Not all YubiKeys support CredentialManagement. To find out if a particular YubiKey can
perform these operations, check for the "`credMgmt`" options.

```csharp
    using (fido2Session = new Fido2Session(yubiKeyDevice))
    {
        if (fido2Session.AuthenticatorInfo.GetOptionValue("credMgmt") == OptionValue.True)
        {
            . . .
        }
    }
```

## Commands and Fido2Session methods

In the SDK, there are two ways to perform a CredentialManagement operation:

* Commands
* Fido2Session methods

The commands are

* [GetCredentialMetadataCommand](xref:Yubico.YubiKey.Fido2.Commands.GetCredentialMetadataCommand)
* [EnumerateRpsBeginCommand](xref:Yubico.YubiKey.Fido2.Commands.EnumerateRpsBeginCommand)
* [EnumerateRpsGetNextCommand](xref:Yubico.YubiKey.Fido2.Commands.EnumerateRpsGetNextCommand)
* [EnumerateCredentialsBeginCommand](xref:Yubico.YubiKey.Fido2.Commands.EnumerateCredentialsBeginCommand)
* [EnumerateCredentialsGetNextCommand](xref:Yubico.YubiKey.Fido2.Commands.EnumerateCredentialsGetNextCommand)
* DeleteCredentialCommand
* UpdateUserInformationCommand

If you call a command, you will be responsible for building a PinToken (see next section).

The Fido2Session methods are

* GetCredentialMetadata
* EnumerateRelyingParties
* EnumerateCredentialsForRelyingParty
* DeleteCredential
* UpdateUserInformationForCredential

If you use these methods, the SDK will build the proper PinToken.

## PIN/UV Auth Param

In order to perform some credential management operations, it is necessary to compute a
PIN/UV Auth Param. The SDK will build the Pin/Uv Auth Param, you do not need to supply it.
The PIN/UV Auth Param is built using a `PinToken`. If you use the Fido2Session methods,
the SDK will also obtain the `PinToken`.

However, the PinToken needed for CredentialManagement must be obtained with the
[CredentialManagement](xref:Yubico.YubiKey.Fido2.Commands.PinUvAuthTokenPermissions)
permission set. Hence, it is possible you have a PinToken already (for example, see the
Fido2Session property [AuthToken](xref:Yubico.YubiKey.Fido2.Fido2Session.AuthToken)), but
need a new one. In order to build a new one, the SDK will need the PIN. Hence, although
you might already have verified the PIN in the session, it can still be necessary to enter
it again.

If you use a command to perform the CredentialManagement operation, you will need to
supply the PinToken. See the
[GetPinUvAuthTokenUsingPinCommand](xref:Yubico.YubiKey.Fido2.Commands.GetPinUvAuthTokenUsingPinCommand).
Your code to get a PinToken will look something like this.

```csharp
    var protocol = new PinUvAuthProtocolTwo();
    var getKeyCmd = new GetKeyAgreementCommand(protocol.Protocol);
    GetKeyAgreementResponse getKeyRsp = connection.SendCommand(getKeyCmd);
    if (getKeyRsp.Status != ResponseStatus.Success)
    {
        ReportError();
        return;
    }
    protocol.Encapsulate(getKeyRsp.GetData());

    PinUvAuthTokenPermissions permissions = PinUvAuthTokenPermissions.CredentialManagement;
    var getTokenCmd = new GetPinUvAuthTokenUsingPinCommand(protocol, pin, permissions, null);
    GetPinUvAuthTokenResponse getTokenRsp = connection.SendCommand(getTokenCmd);
    if (getTokenRsp.Status != ResponseStatus.Success)
    {
        ReportError();
        return;
    }

    ReadOnlyMemory<byte> pinToken = getTokenRsp.GetData();
```

## Get metadata

This returns the number of discoverable credentials and the number of "empty" slots. For
example, suppose the YubiKey has space for 25 credentials. Currently there are three
discoverable credentials and two non-discoverable. The return from the credential
management operation of get metadata would be 3 and 20. The number of remaining credential
count of 20 means that it is possible to store 20 more credentials, any combination of
discoverable and non-discoverable.

The return is a `CredentialManagementData` object, with the properties set as follows.

```txt
  `NumberOfDiscoverableCredentials  = 3`
  `RemainingCredentialCount = 20`
  `RelyingParty = null`
  `RelyingPartyIdHash = null`
  `TotalRelyingPartyCount = null`
  `User = null`
  `CredentialId = null`
  `CredentialPublicKey = null`
  `TotalCredentialsForRelyingParty = null`
  `CredProtectPolicy = null`
  `LargeBlobKey = null`
```

## Enumerate relying parties

This helps you to build a list of all the relying parties represented among all the
credentials on the YubiKey.

If you use Fido2Session.EnumerateRelyingParties, the SDK will return an array of
`CredentialManagementData` objects, each one containing the `RelyingParty` and the
`RelyingPartyIdHash` for each relying party found on the YubiKey. You will likely look at
the `RelyingParty` property of each for information that is used to choose which one you
are interested in. Then you will use the `RelyingPartyIdHash` in further calls to
enumerate the credentials for that relying party (see the next section).

If you use the commands, you will need to use the `EnumerateRpsBeginCommand` command to
obtain the first relying party and the total count of relying parties represented, and
then the `EnumerateRpsGetNextCommand` to get each successive relying party.

```csharp
    var enumBeginCmd = new EnumerateRpsBeginCommand(pinToken, protocol);
    CredentialManagementResponse credMgmtRsp = connection.SendCommand(enumBeginCmd);

    CredentialManagementData mgmtData = credMgmtRsp.GetData();
    int count = mgmtData.TotalRelyingPartyCount ?? 0;

    for (int index = 1; index < count; index++)
    {
        var getNextCmd = new EnumerateRpsGetNextCommand();
        CredentialManagementResponse credMgmtRsp = connection.SendCommand(getNextCmd);
        mgmtData = getNextRsp.GetData();
    }
```

## Enumerate credentials

This helps you to build a list of all the credentials on the YubiKey.

If you use Fido2Session.EnumerateCredentialsForRelyingParty, the SDK will return an array
of `CredentialManagementData` objects, each one containing the `User`, `CredentialId`, and
`CredentialPublicKey` for each credential found on the YubiKey associated with the
specified relying party. You specify which relying party you are interested in by
supplying the `RelyingPartyIdHash`, which you likely retrieved during a call to obtain a
list of relying parties.

If you use the commands, you will need to use the `EnumerateCredentialsBeginCommand`
command to obtain the first credential and the total count of credentials available, and
then the `EnumerateCredentialsGetNextCommand` to get each successive credential.

```csharp
    var enumBeginCmd = new EnumerateCredentialsBeginCommand(relyingPartyIdHash, pinToken, protocol);
    CredentialManagementResponse credMgmtRsp = connection.SendCommand(enumBeginCmd);

    CredentialManagementData mgmtData = credMgmtRsp.GetData();
    int count = mgmtData.TotalCredentialsForRelyingParty ?? 0;

    for (int index = 1; index < count; index++)
    {
        var getNextCmd = new EnumerateCredentialsGetNextCommand();
        CredentialManagementResponse credMgmtRsp = connection.SendCommand(getNextCmd);
        mgmtData = getNextRsp.GetData();
    }
```

## Delete credential

Remove one credential from the YubiKey.

## Update user information

Each credential contains user information, represented as an instance of the
[UserEntity](xref:Yubico.YubiKey.Fido2.UserEntity) class. You can change what user
information is stored on the YubiKey in that credential.