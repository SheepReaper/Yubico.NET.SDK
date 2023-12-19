// Copyright 2022 Yubico AB
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using Xunit;
using Yubico.YubiKey.Fido2.Commands;
using Yubico.YubiKey.TestUtilities;

namespace Yubico.YubiKey.Fido2
{
    public class CredMgmtTests : IClassFixture<BioFido2Fixture>
    {
        private readonly BioFido2Fixture _bioFido2Fixture;

        public CredMgmtTests(BioFido2Fixture bioFido2Fixture)
        {
            _bioFido2Fixture = bioFido2Fixture;

            if (!_bioFido2Fixture.HasCredentials)
            {
                _bioFido2Fixture.AddCredentials(2, 1);
                _bioFido2Fixture.AddCredentials(1, 0);
            }
        }

        [Fact]
        public void GetMetadata_Succeeds()
        {
            using (var fido2Session = new Fido2Session(_bioFido2Fixture.Device))
            {
                fido2Session.KeyCollector = _bioFido2Fixture.KeyCollector;
                fido2Session.AddPermissions(
                    PinUvAuthTokenPermissions.MakeCredential, _bioFido2Fixture.RpInfoList[0].RelyingParty.Id);

                (int credCount, int slotCount) = fido2Session.GetCredentialMetadata();
                Assert.Equal(3, credCount);
                Assert.Equal(22, slotCount);
            }
        }

        [Fact]
        public void EnumerateRps_Succeeds()
        {
            using (var fido2Session = new Fido2Session(_bioFido2Fixture.Device))
            {
                fido2Session.KeyCollector = _bioFido2Fixture.KeyCollector;
                fido2Session.AddPermissions(PinUvAuthTokenPermissions.CredentialManagement, "rp-3");

                IReadOnlyList<RelyingParty> rpList = fido2Session.EnumerateRelyingParties();
                Assert.Equal(2, rpList.Count);

                RpInfo rpInfo = _bioFido2Fixture.MatchRelyingParty(rpList[0]);
                bool isValid = MemoryExtensions.SequenceEqual<byte>(
                    rpInfo.RelyingPartyIdHash.Span, rpList[0].RelyingPartyIdHash.Span);
                Assert.True(isValid);
            }
        }

        [Fact]
        public void EnumerateCreds_Succeeds()
        {
            using (var fido2Session = new Fido2Session(_bioFido2Fixture.Device))
            {
                fido2Session.KeyCollector = _bioFido2Fixture.KeyCollector;
                fido2Session.AddPermissions(PinUvAuthTokenPermissions.MakeCredential, "rp-3");

                RpInfo rpInfo = _bioFido2Fixture.RpInfoList[0];
                IReadOnlyList<CredentialUserInfo> ykCredList =
                    fido2Session.EnumerateCredentialsForRelyingParty(rpInfo.RelyingParty);
                Assert.Equal(rpInfo.DiscoverableCount, ykCredList.Count);

                UserEntity ykUser = ykCredList[0].User;

                Tuple<UserEntity,MakeCredentialData> userInfo = _bioFido2Fixture.MatchUser(rpInfo.RelyingParty, ykUser);
                ReadOnlyMemory<byte> targetKey = userInfo.Item2.LargeBlobKey
                    ?? throw new InvalidOperationException("No matching User.");
                ReadOnlyMemory<byte> ykLargeBlobKey = ykCredList[0].LargeBlobKey
                    ?? throw new InvalidOperationException("No matching User.");

                bool isValid = MemoryExtensions.SequenceEqual<byte>(targetKey.Span, ykLargeBlobKey.Span);
                Assert.True(isValid);
            }
        }

        [Fact]
        public void DeleteCred_Succeeds()
        {
            _bioFido2Fixture.AddCredentials(1, 0);
            using (var fido2Session = new Fido2Session(_bioFido2Fixture.Device))
            {
                fido2Session.KeyCollector = _bioFido2Fixture.KeyCollector;
                fido2Session.AddPermissions(PinUvAuthTokenPermissions.AuthenticatorConfiguration, null);

                IReadOnlyList<CredentialUserInfo> credList =
                    fido2Session.EnumerateCredentialsForRelyingParty(_bioFido2Fixture.RpInfoList[2].RelyingParty);
                int count = credList.Count;
                Assert.Equal(1, count);

                fido2Session.ClearAuthToken();
                fido2Session.AddPermissions(PinUvAuthTokenPermissions.AuthenticatorConfiguration, null);

                fido2Session.DeleteCredential(credList[0].CredentialId);

                credList = fido2Session.EnumerateCredentialsForRelyingParty(_bioFido2Fixture.RpInfoList[2].RelyingParty);
                Assert.NotNull(credList);
                Assert.True(credList.Count == count - 1);
            }
        }

        [Fact]
        public void UpdateUserInfo_Succeeds()
        {
            string updatedDisplayName = "Updated Display Name";

            using (var fido2Session = new Fido2Session(_bioFido2Fixture.Device))
            {
                fido2Session.KeyCollector = _bioFido2Fixture.KeyCollector;
                fido2Session.AddPermissions(PinUvAuthTokenPermissions.AuthenticatorConfiguration, null);

                IReadOnlyList<CredentialUserInfo> credList =
                    fido2Session.EnumerateCredentialsForRelyingParty(_bioFido2Fixture.RpInfoList[0].RelyingParty);
                Assert.NotEmpty(credList);

                fido2Session.ClearAuthToken();
                fido2Session.AddPermissions(PinUvAuthTokenPermissions.AuthenticatorConfiguration, null);

                UserEntity newInfo = credList[0].User;
                newInfo.DisplayName = updatedDisplayName;

                fido2Session.UpdateUserInfoForCredential(credList[0].CredentialId, newInfo);

                credList = fido2Session.EnumerateCredentialsForRelyingParty(_bioFido2Fixture.RpInfoList[0].RelyingParty);

                string displayName = credList[0].User.DisplayName??"";
                Assert.Equal(displayName, updatedDisplayName);
            }
        }
    }
}
