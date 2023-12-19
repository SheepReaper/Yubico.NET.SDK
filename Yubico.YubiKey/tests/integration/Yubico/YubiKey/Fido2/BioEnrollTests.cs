// Copyright 2023 Yubico AB
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
using System.Text;
using System.Collections.Generic;
using Xunit;
using Yubico.YubiKey.Fido2.Commands;
using Yubico.YubiKey.TestUtilities;

namespace Yubico.YubiKey.Fido2
{
    public class BioEnrollTests : SimpleIntegrationTestConnection
    {
        // Set to 0 meaning don't cancel.
        // Or set to an integer indicating that the enroll fingerprint call
        // should be canceled when the remaining sample count gets to that value.
        // Or set to -1, meaning cancel on the first call to the KeyCollector
        // (cancel before any fingerprints are sampled).
        // If the call is touch or verify fingerprint, then any non-zero
        // _callCancelCount value means cancel when the request comes in.
        private int _callCancelCount = 0;

        public BioEnrollTests()
            : base(YubiKeyApplication.Fido2, StandardTestDevice.Bio)
        {
        }

        [Fact]
        public void GetModality_Succeeds()
        {
            using (var fido2Session = new Fido2Session(Device))
            {
                BioModality modality = fido2Session.GetBioModality();

                Assert.Equal(BioModality.Fingerprint, modality);
            }
        }

        [Fact]
        public void GetSensorInfo_Succeeds()
        {
            using (var fido2Session = new Fido2Session(Device))
            {
                FingerprintSensorInfo sensorInfo = fido2Session.GetFingerprintSensorInfo();

                Assert.Equal(1, sensorInfo.FingerprintKind);
                Assert.Equal(16, sensorInfo.MaxCaptureCount);
                Assert.Equal(15, sensorInfo.MaxFriendlyNameBytes);
            }
        }

        [Fact]
        public void EnrollFingerprint_Succeeds()
        {
            string firstName = "SomeName";
            string secondName = "Another Name";
            _callCancelCount = 0;

            using (var fido2Session = new Fido2Session(Device))
            {
                fido2Session.KeyCollector = LocalKeyCollector;

                TemplateInfo templateInfo = fido2Session.EnrollFingerprint(firstName, 5000);

                Assert.NotNull(templateInfo.FriendlyName);
                Assert.Equal(templateInfo.FriendlyName, firstName);
                Assert.NotEmpty(templateInfo.TemplateId.ToArray());

                fido2Session.SetBioTemplateFriendlyName(templateInfo.TemplateId, secondName);

                IReadOnlyList<TemplateInfo> fpList = fido2Session.EnumerateBioEnrollments();
                bool isValid = false;
                foreach (TemplateInfo info in fpList)
                {
                    if (info.FriendlyName.Equals(secondName))
                    {
                        isValid = MemoryExtensions.SequenceEqual<byte>(
                            info.TemplateId.Span, templateInfo.TemplateId.Span);

                        if (isValid)
                        {
                            break;
                        }
                    }
                }

                Assert.True(isValid);

                isValid = fido2Session.TryRemoveBioTemplate(templateInfo.TemplateId);
                Assert.True(isValid);

                fpList = fido2Session.EnumerateBioEnrollments();
                // isValid is currently true. If the template was deleted, that
                // means the Remove worked, so leave it at true. If the template
                // is still there, it failed, so set it to false.
                foreach (TemplateInfo info in fpList)
                {
                    if (MemoryExtensions.SequenceEqual<byte>(
                        info.TemplateId.Span, templateInfo.TemplateId.Span))
                    {
                        isValid = false;
                        break;
                    }
                }

                Assert.True(isValid);
            }
        }

        [Theory]
        [InlineData(4)]
        [InlineData(-1)]
        public void EnrollFingerprint_Cancel_ThrowsCorrect(int cancelCount)
        {
            _callCancelCount = cancelCount;

            using (var fido2Session = new Fido2Session(Device))
            {
                fido2Session.KeyCollector = LocalKeyCollector;

                _ = Assert.Throws<OperationCanceledException>(() => fido2Session.EnrollFingerprint(null, 5000));
            }
        }

        private bool LocalKeyCollector(KeyEntryData arg)
        {
            bool callCancel = (_callCancelCount == 0) ? false : true;

            if ((_callCancelCount > 0) && (!(arg.LastBioEnrollSampleResult is null)))
            {
                if (arg.LastBioEnrollSampleResult.RemainingSampleCount != _callCancelCount)
                {
                    callCancel = false;
                }
            }

            switch (arg.Request)
            {
                case KeyEntryRequest.TouchRequest:
                    Console.WriteLine("YubiKey requires touch");
                    break;
                case KeyEntryRequest.VerifyFido2Pin:
                    arg.SubmitValue(Encoding.UTF8.GetBytes("123456"));
                    break;
                case KeyEntryRequest.VerifyFido2Uv:
                    if (callCancel && !(arg.SignalUserCancel is null))
                    {
                        arg.SignalUserCancel();
                    }
                    Console.WriteLine("Fingerprint requested.");
                    break;
                case KeyEntryRequest.EnrollFingerprint:
                    if (callCancel && !(arg.SignalUserCancel is null))
                    {
                        arg.SignalUserCancel();
                    }
                    Console.WriteLine("Fingerprint needed.");
                    break;
                case KeyEntryRequest.Release:
                    break;
                default:
                    throw new NotSupportedException("Not supported by this test");
            }

            return true;
        }
    }
}
