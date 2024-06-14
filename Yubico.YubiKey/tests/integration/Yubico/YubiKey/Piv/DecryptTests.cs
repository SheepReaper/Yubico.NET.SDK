// Copyright 2021 Yubico AB
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
using System.Linq;
using System.Security.Cryptography;
using Xunit;
using Yubico.YubiKey.Cryptography;
using Yubico.YubiKey.TestUtilities;

namespace Yubico.YubiKey.Piv
{
    [Trait("Category", "Simple")]
    public class DecryptTests
    {
        [Theory]
        [InlineData(PivPinPolicy.Always, StandardTestDevice.Fw5)]
        [InlineData(PivPinPolicy.Never, StandardTestDevice.Fw5)]
        public void Decrypt_1024_Succeeds(PivPinPolicy pinPolicy, StandardTestDevice testDeviceType)
        {
            byte[] dataToDecrypt = {
                0x64, 0x92, 0xd1, 0x38, 0x24, 0x8a, 0x78, 0xe5, 0x64, 0x68, 0x92, 0xe7, 0x13, 0xc6, 0x81, 0xa0,
                0xe9, 0xeb, 0x43, 0x8f, 0x54, 0x76, 0x55, 0x84, 0x16, 0x3e, 0x47, 0x76, 0x31, 0x6a, 0xc2, 0x7d,
                0x27, 0x0f, 0x6c, 0x4f, 0xd5, 0x17, 0x52, 0xea, 0x3e, 0xce, 0xe5, 0xd6, 0x5c, 0x09, 0xac, 0xc2,
                0xb1, 0xea, 0xbb, 0x5f, 0x05, 0x16, 0x9f, 0x2e, 0x05, 0x20, 0x3a, 0x28, 0x90, 0x76, 0xcf, 0x72,
                0xea, 0x15, 0x7e, 0x8a, 0x5a, 0x05, 0x0f, 0xc0, 0x80, 0x54, 0x18, 0x2f, 0x8f, 0xbb, 0xf1, 0xcf,
                0x1a, 0xe9, 0xe6, 0x82, 0x4d, 0xef, 0xdb, 0x52, 0xff, 0x7a, 0xd4, 0x3c, 0x7d, 0xd1, 0x21, 0x53,
                0xbd, 0x2c, 0x74, 0x25, 0xe0, 0x36, 0xcc, 0x79, 0x85, 0x7e, 0x52, 0x85, 0xae, 0xd5, 0xf7, 0x40,
                0x5a, 0xa7, 0x94, 0xde, 0x68, 0x1b, 0xaa, 0x8b, 0x58, 0x95, 0x04, 0x22, 0xd6, 0xfc, 0x3f, 0xbc
            };

            _ = SampleKeyPairs.GetKeysAndCertPem(PivAlgorithm.Rsa1024, false, out _, out _, out var privateKeyPem);
            var privateKey = new KeyConverter(privateKeyPem.ToCharArray());
            var pivPrivateKey = privateKey.GetPivPrivateKey();

            var testDevice = IntegrationTestDeviceEnumeration.GetTestDevice(testDeviceType);

            using (var pivSession = new PivSession(testDevice))
            {
                var collectorObj = new Simple39KeyCollector();
                pivSession.KeyCollector = collectorObj.Simple39KeyCollectorDelegate;

                pivSession.ImportPrivateKey(0x89, pivPrivateKey, pinPolicy, PivTouchPolicy.Never);

                var decryptedData = pivSession.Decrypt(0x89, dataToDecrypt);
                Assert.Equal(dataToDecrypt.Length, decryptedData.Length);
            }
        }
        [Theory]
        [InlineData(PivPinPolicy.Always, StandardTestDevice.Fw5)]
        [InlineData(PivPinPolicy.Never, StandardTestDevice.Fw5)]
        public void Decrypt_2048_Succeeds(PivPinPolicy pinPolicy, StandardTestDevice testDeviceType)
        {
            byte[] dataToDecrypt = {
                0x64, 0x92, 0xd1, 0x38, 0x24, 0x8a, 0x78, 0xe5, 0x64, 0x68, 0x92, 0xe7, 0x13, 0xc6, 0x81, 0xa0,
                0xe9, 0xeb, 0x43, 0x8f, 0x54, 0x76, 0x55, 0x84, 0x16, 0x3e, 0x47, 0x76, 0x31, 0x6a, 0xc2, 0x7d,
                0x27, 0x0f, 0x6c, 0x4f, 0xd5, 0x17, 0x52, 0xea, 0x3e, 0xce, 0xe5, 0xd6, 0x5c, 0x09, 0xac, 0xc2,
                0xb1, 0xea, 0xbb, 0x5f, 0x05, 0x16, 0x9f, 0x2e, 0x05, 0x20, 0x3a, 0x28, 0x90, 0x76, 0xcf, 0x72,
                0xea, 0x15, 0x7e, 0x8a, 0x5a, 0x05, 0x0f, 0xc0, 0x80, 0x54, 0x18, 0x2f, 0x8f, 0xbb, 0xf1, 0xcf,
                0x1a, 0xe9, 0xe6, 0x82, 0x4d, 0xef, 0xdb, 0x52, 0xff, 0x7a, 0xd4, 0x3c, 0x7d, 0xd1, 0x21, 0x53,
                0xbd, 0x2c, 0x74, 0x25, 0xe0, 0x36, 0xcc, 0x79, 0x85, 0x7e, 0x52, 0x85, 0xae, 0xd5, 0xf7, 0x40,
                0x5a, 0xa7, 0x94, 0xde, 0x68, 0x1b, 0xaa, 0x8b, 0x58, 0x95, 0x04, 0x22, 0xd6, 0xfc, 0x3f, 0xbc,
                0x8B, 0x1C, 0x84, 0x52, 0x7E, 0x02, 0x89, 0x9F, 0x58, 0x5C, 0xFF, 0xDB, 0x35, 0x48, 0xC3, 0x6E,
                0xBC, 0x29, 0xFC, 0xE7, 0xAC, 0x3E, 0x44, 0xCC, 0xC4, 0x21, 0xFA, 0xCB, 0xAA, 0x98, 0x47, 0x5F,
                0xB4, 0x76, 0x4E, 0x8F, 0x3D, 0x3E, 0xE8, 0xC1, 0xBE, 0xFB, 0x55, 0x48, 0x82, 0xE6, 0xAD, 0x9A,
                0x40, 0x32, 0x49, 0xC4, 0xC6, 0x10, 0xC5, 0x03, 0xCD, 0x66, 0xDB, 0x81, 0x02, 0x21, 0x00, 0xE0,
                0x8C, 0x19, 0x1D, 0x98, 0xB8, 0xC1, 0xB2, 0x0E, 0x6B, 0xD5, 0x4E, 0x20, 0xCE, 0x60, 0xCB, 0x1E,
                0x4d, 0xef, 0xdb, 0x52, 0x48, 0x82, 0xE6, 0xAD, 0x76, 0x31, 0x6a, 0xc2, 0x89, 0x9F, 0x58, 0x5C,
                0x71, 0x2F, 0xB4, 0xE9, 0x2D, 0xE0, 0x51, 0x5B, 0xCD, 0xDE, 0xBF, 0x3C, 0xE7, 0x9A, 0x71, 0x02,
                0x21, 0x00, 0xC5, 0xCD, 0x80, 0x23, 0x17, 0x2D, 0xB0, 0xFE, 0x9D, 0xF0, 0x28, 0x6C, 0x50, 0xBD
            };

            _ = SampleKeyPairs.GetKeysAndCertPem(PivAlgorithm.Rsa2048, false, out _, out _, out var privateKeyPem);
            var privateKey = new KeyConverter(privateKeyPem.ToCharArray());
            var pivPrivateKey = privateKey.GetPivPrivateKey();

            var testDevice = IntegrationTestDeviceEnumeration.GetTestDevice(testDeviceType);

            using (var pivSession = new PivSession(testDevice))
            {
                var collectorObj = new Simple39KeyCollector();
                pivSession.KeyCollector = collectorObj.Simple39KeyCollectorDelegate;

                pivSession.ImportPrivateKey(0x87, pivPrivateKey, pinPolicy, PivTouchPolicy.Never);

                var decryptedData = pivSession.Decrypt(0x87, dataToDecrypt);
                Assert.Equal(dataToDecrypt.Length, decryptedData.Length);
            }
        }

        [Theory]
        [InlineData(PivAlgorithm.Rsa1024, 0x94, RsaFormat.Sha1, 1, StandardTestDevice.Fw5)]
        [InlineData(PivAlgorithm.Rsa1024, 0x94, RsaFormat.Sha1, 2, StandardTestDevice.Fw5)]
        [InlineData(PivAlgorithm.Rsa1024, 0x94, RsaFormat.Sha256, 2, StandardTestDevice.Fw5)]
        [InlineData(PivAlgorithm.Rsa1024, 0x94, RsaFormat.Sha384, 2, StandardTestDevice.Fw5)]
        [InlineData(PivAlgorithm.Rsa2048, 0x95, RsaFormat.Sha1, 1, StandardTestDevice.Fw5)]
        [InlineData(PivAlgorithm.Rsa2048, 0x95, RsaFormat.Sha1, 2, StandardTestDevice.Fw5)]
        [InlineData(PivAlgorithm.Rsa2048, 0x95, RsaFormat.Sha256, 2, StandardTestDevice.Fw5)]
        [InlineData(PivAlgorithm.Rsa2048, 0x95, RsaFormat.Sha384, 2, StandardTestDevice.Fw5)]
        [InlineData(PivAlgorithm.Rsa2048, 0x95, RsaFormat.Sha512, 2, StandardTestDevice.Fw5)]
        public void EncryptCSharp_Decrypt_Correct(PivAlgorithm algorithm, byte slotNumber, int digestAlgorithm, int paddingScheme, StandardTestDevice testDeviceType)
        {
            var rsaPadding = RSAEncryptionPadding.Pkcs1;
            if (paddingScheme != 1)
            {
                rsaPadding = digestAlgorithm switch
                {
                    RsaFormat.Sha256 => RSAEncryptionPadding.OaepSHA256,
                    RsaFormat.Sha384 => RSAEncryptionPadding.OaepSHA384,
                    RsaFormat.Sha512 => RSAEncryptionPadding.OaepSHA512,
                    _ => RSAEncryptionPadding.OaepSHA1,
                };
            }

            var dataToEncrypt = new byte[16];
            GetArbitraryData(dataToEncrypt);

            _ = SampleKeyPairs.GetKeysAndCertPem(algorithm, false, out _, out var pubKeyPem, out var priKeyPem);
            var pubKey = new KeyConverter(pubKeyPem.ToCharArray());
            var priKey = new KeyConverter(priKeyPem.ToCharArray());

            using var rsaObject = pubKey.GetRsaObject();
            var encryptedData = rsaObject.Encrypt(dataToEncrypt, rsaPadding);

            var pivPrivateKey = priKey.GetPivPrivateKey();
            var testDevice = IntegrationTestDeviceEnumeration.GetTestDevice(testDeviceType);

            using (var pivSession = new PivSession(testDevice))
            {
                var collectorObj = new Simple39KeyCollector();
                pivSession.KeyCollector = collectorObj.Simple39KeyCollectorDelegate;

                pivSession.ImportPrivateKey(slotNumber, pivPrivateKey, PivPinPolicy.Default, PivTouchPolicy.Never);

                var formattedData = pivSession.Decrypt(slotNumber, encryptedData);

                byte[] decryptedData;
                bool isValid;
                if (paddingScheme == 1)
                {
                    isValid = RsaFormat.TryParsePkcs1Decrypt(formattedData, out decryptedData);
                }
                else
                {
                    isValid = RsaFormat.TryParsePkcs1Oaep(formattedData, digestAlgorithm, out decryptedData);
                }
                Assert.True(isValid);

                isValid = dataToEncrypt.SequenceEqual(decryptedData);
                Assert.True(isValid);
            }
        }

        [Theory]
        [InlineData(StandardTestDevice.Fw5)]
        public void NoKeyInSlot_Decrypt_Exception(StandardTestDevice testDeviceType)
        {
            var dataToDecrypt = new byte[256];
            GetArbitraryData(dataToDecrypt);

            var testDevice = IntegrationTestDeviceEnumeration.GetTestDevice(testDeviceType);

            using (var pivSession = new PivSession(testDevice))
            {
                var collectorObj = new Simple39KeyCollector();
                pivSession.KeyCollector = collectorObj.Simple39KeyCollectorDelegate;

                pivSession.ResetApplication();

                _ = Assert.Throws<InvalidOperationException>(() => pivSession.Decrypt(0x9a, dataToDecrypt));
            }
        }

        // Fill a byte array with "random" data. Up to 256 bytes.
        private static void GetArbitraryData(byte[] bufferToFill)
        {
            byte[] arbitraryData = {
                0x3E, 0xE8, 0xC1, 0xBE, 0xFB, 0x55, 0x48, 0x82, 0xE6, 0xAD, 0x9A, 0xBC, 0x84, 0x04, 0xF4, 0xA4,
                0xF0, 0xE3, 0x08, 0x53, 0x02, 0x03, 0x01, 0x00, 0x01, 0x02, 0x41, 0x00, 0xAA, 0xA0, 0xBB, 0x04,
                0x9E, 0xD7, 0xBA, 0x33, 0x0D, 0x44, 0x84, 0xEC, 0x30, 0x0A, 0xB0, 0x8E, 0xF2, 0x47, 0x1D, 0x89,
                0xF5, 0x99, 0x5D, 0x99, 0xE7, 0xA1, 0x35, 0x26, 0x0B, 0xC7, 0x15, 0xA8, 0x5E, 0x75, 0x55, 0x63,
                0x1A, 0x89, 0xD8, 0x0E, 0x55, 0xD9, 0x1C, 0x89, 0x8A, 0xF4, 0xDE, 0x54, 0x05, 0xA5, 0x53, 0xA0,
                0x40, 0x32, 0x49, 0xC4, 0xC6, 0x10, 0xC5, 0x03, 0xCD, 0x66, 0xDB, 0x81, 0x02, 0x21, 0x00, 0xE0,
                0x8C, 0x19, 0x1D, 0x98, 0xB8, 0xC1, 0xB2, 0x0E, 0x6B, 0xD5, 0x4E, 0x20, 0xCE, 0x60, 0xCB, 0x1E,
                0x71, 0x2F, 0xB4, 0xE9, 0x2D, 0xE0, 0x51, 0x5B, 0xCD, 0xDE, 0xBF, 0x3C, 0xE7, 0x9A, 0x71, 0x02,
                0x21, 0x00, 0xC5, 0xCD, 0x80, 0x23, 0x17, 0x2D, 0xB0, 0xFE, 0x9D, 0xF0, 0x28, 0x6C, 0x50, 0xBE,
                0x66, 0x31, 0x28, 0x76, 0xC0, 0x86, 0x9B, 0x69, 0xDB, 0xD9, 0xA8, 0x47, 0xD1, 0xAC, 0x3E, 0x42,
                0x49, 0x03, 0x02, 0x21, 0x00, 0xCE, 0xBB, 0xED, 0xBB, 0xB4, 0x0A, 0x16, 0x3B, 0x0A, 0xCF, 0xF8,
                0xF9, 0x0F, 0x77, 0x32, 0xE2, 0x8F, 0x4A, 0x82, 0x33, 0xBB, 0xA3, 0x83, 0x2D, 0x24, 0xAA, 0xAB,
                0xF3, 0xC1, 0xED, 0x31, 0xE1, 0x02, 0x20, 0x58, 0x44, 0x4C, 0xC2, 0xDB, 0xEC, 0x02, 0xC8, 0x8C,
                0x38, 0x08, 0x01, 0xD5, 0xC2, 0x31, 0x1E, 0x0C, 0x9D, 0x79, 0x6A, 0x57, 0xDD, 0xD4, 0x42, 0x7B,
                0x8B, 0x1C, 0x84, 0x52, 0x7E, 0x02, 0x89, 0x9F, 0x58, 0x5C, 0xFF, 0xDB, 0x35, 0x48, 0xC3, 0x6E,
                0xBC, 0x29, 0xFC, 0xE7, 0xAC, 0x3E, 0x44, 0xCC, 0xC4, 0x21, 0xFA, 0xCB, 0xAA, 0x98, 0x47, 0x5F
            };

            var count = 256;
            if (bufferToFill.Length < 256)
            {
                count = bufferToFill.Length;
            }

            Array.Copy(arbitraryData, 0, bufferToFill, 0, count);
        }
    }
}
