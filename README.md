# SoulsFormats
A .NET library for reading and writing various FromSoftware file formats, targeting .NET Framework 4.6 and .NET Standard 2.1.  
Dark Souls, Demon's Souls, Bloodborne, and Sekiro are the main focus, but other From games may be supported to varying degrees.  
A brief description of each supported format can be found in FORMATS.md, with further documentation for some formats.  

## Usage
Objects for most formats can be created with the static method Read, which accepts either a byte array or a file path. Using a path is preferable as it will be read with a stream, reducing memory consumption.
```cs
BND3 bnd = BND3.Read(@"C:\your\path\here.chrbnd");

// or

byte[] bytes = File.ReadAllBytes(@"C:\your\path\here.chrbnd");
BND3 bnd = BND3.Read(bytes);
```

The Write method can be used to create a new file from an object. If given a path it will be written to that location with a stream, otherwise a byte array will be returned.
```cs
bnd.Write(@"C:\your\path\here.chrbnd");

// or

byte[] bytes = bnd.Write();
File.WriteAllBytes(@"C:\your\path\here.chrbnd", bytes);
```

DCX (compressed files) will be decompressed automatically and the compression type will be remembered and reapplied when writing the file.
```cs
BND3 bnd = BND3.Read(@"C:\your\path\here.chrbnd.dcx");
bnd.Write(@"C:\your\path\here.chrbnd.dcx");
```

The compression type can be changed by either setting the Compression field of the object, or specifying one when calling Write.
```cs
BND3 bnd = BND3.Read(@"C:\your\path\here.chrbnd.dcx");
bnd.Write(@"C:\your\path\here.chrbnd", DCX.Type.None);

// or

BND3 bnd = BND3.Read(@"C:\your\path\here.chrbnd.dcx");
bnd.Compression = DCX.Type.None;
bnd.Write(@"C:\your\path\here.chrbnd");
```

Finally, DCX files can be generically read and written with static methods if necessary. DCX holds no important metadata so they read/write directly to/from byte arrays instead of creating an object.
```cs
byte[] bndBytes = DCX.Decompress(@"C:\your\path\here.chrbnd.dcx");
BND3 bnd = BND3.Read(bndBytes);

// or

byte[] dcxBytes = File.ReadAllBytes(@"C:\your\path\here.chrbnd.dcx");
byte[] bndBytes = DCX.Decompress(dcxBytes);
BND3 bnd = BND3.Read(bndBytes);
```

Writing a new DCX requires a DCX.Type parameter indicating which game it is for. DCX.Decompress has an optional out parameter indicating the detected type which should usually be used instead of specifying your own.
```cs
byte[] bndBytes = DCX.Decompress(@"C:\your\path\here.chrbnd.dcx", out DCX.Type type);
DCX.Compress(bndBytes, type, @"C:\your\path\here.chrbnd.dcx");

// or

byte[] bndBytes = DCX.Decompress(@"C:\your\path\here.chrbnd.dcx", out DCX.Type type);
byte[] dcxBytes = DCX.Compress(bndBytes, type);
File.WriteAllBytes(@"C:\your\path\here.chrbnd.dcx", dcxBytes);
```

## Special Thanks
To everyone below, for either creating tools that I learned from, or helping decipher these formats one way or another. Please yell at me on Discord if I missed you.
* albeartron
* Atvaark
* B3LYP
* horkrux
* HotPocketRemix
* katalash
* Lance
* Meowmaritus
* Nyxojaele
* Pav
* SeanP
* thefifthmatt
* Wulf2k
* Yoshimitsu
