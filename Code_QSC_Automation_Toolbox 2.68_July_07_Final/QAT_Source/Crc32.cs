using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace QSC_Test_Automation
{
  public class Crc32 : HashAlgorithm
  {

    public const UInt32 DefaultPolynomial = 0xedb88320;
    public const UInt32 DefaultSeed = 0xffffffff;

    private UInt32 hash;
    private UInt32 seed;
    private UInt32[] table;
    private static UInt32[] defaultTable;

    public Crc32()
    {
      table = InitializeTable(DefaultPolynomial);
      seed = DefaultSeed;
      Initialize();
    }

    public Crc32(UInt32 polynomial, UInt32 seed)
    {
      table = InitializeTable(polynomial);
      this.seed = seed;
      Initialize();
    }

    public override void Initialize()
    {
      hash = seed;
    }

    protected override void HashCore(byte[] buffer, int start, int length)
    {
      hash = CalculateHash(table, hash, buffer, start, length);
    }

    protected override byte[] HashFinal()
    {
      byte[] hashBuffer = UInt32ToBigEndianBytes(~hash);
      this.HashValue = hashBuffer;
      return hashBuffer;
    }

    public override int HashSize
    {
      get { return 32; }
    }

    public static UInt32 Compute(byte[] buffer)
    {
      return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
    }

    public static UInt32 Compute(UInt32 seed, byte[] buffer)
    {
      return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
    }

    public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
    {
      return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
    }

    private static UInt32[] InitializeTable(UInt32 polynomial)
    {
      if (polynomial == DefaultPolynomial && defaultTable != null)
        return defaultTable;

      UInt32[] createTable = new UInt32[256];
      for (int i = 0; i < 256; i++)
      {
        UInt32 entry = (UInt32)i;
        for (int j = 0; j < 8; j++)
          if ((entry & 1) == 1)
            entry = (entry >> 1) ^ polynomial;
          else
            entry = entry >> 1;
        createTable[i] = entry;
      }

      if (polynomial == DefaultPolynomial)
        defaultTable = createTable;

      return createTable;
    }

    private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, byte[] buffer, int start, int size)
    {
      UInt32 crc = seed;
      for (int i = start; i < size; i++)
        unchecked
        {
          crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
        }
      return crc;
    }

    private byte[] UInt32ToBigEndianBytes(UInt32 x)
    {
      return new byte[] {
			(byte)((x >> 24) & 0xff),
			(byte)((x >> 16) & 0xff),
			(byte)((x >> 8) & 0xff),
			(byte)(x & 0xff)
		};
    }

    public string CalculateHash(byte[] bytes)
    {
      string hash = string.Empty;
      Crc32 serverCrc = new Crc32();
      foreach (byte b in serverCrc.ComputeHash(bytes)) hash += b.ToString("x2").ToLower();
      return hash;
    }

      /// <summary>
      /// Method Used to Calculate the Local Hash on the Computer
      /// </summary>
      /// <param name="filename">Path to .bin file</param>
      /// <returns>string representation of the hash</returns>
    public string CalculateHash(string filename)
    {
      string hash = string.Empty;
      Crc32 crc32 = new Crc32();
      using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        foreach (byte b in crc32.ComputeHash(fs)) hash += b.ToString("x2").ToLower();
      }
      return hash;
    }

        public string GetMD5HashFromFile(string fileName)
        {
           
            byte[] myFileData = File.ReadAllBytes(fileName);
            byte[] myHash;
            using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                 myHash = MD5.Create().ComputeHash(fs);
            }
                
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < myHash.Length; i++)
            {
                sb.Append(myHash[i].ToString());
            }
            return sb.ToString();
        }
    }
}
