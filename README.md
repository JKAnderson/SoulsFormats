
# SoulsFormats
A .NET library for reading and writing various FromSoftware file formats, targeting .NET Framework 4.5.  
Dark Souls 1, Dark Souls Remastered, Dark Souls 2, Dark Souls 3, Demon's Souls, Bloodborne, and Ninja Blade are all supported to varying degrees. See below for a breakdown of each format.

## Usage
Objects for each format can be created with the static method Read, which accepts either a byte array or a file path. Using a path is preferable as it will be read with a stream, reducing memory consumption.
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

DCX (compressed files) hold no important metadata so they read/write directly to/from byte arrays instead of creating an object.
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

## Formats
### BND3
A general-purpose file container used before DS2.  
Extension: `.*bnd`
* DS1: Full Read and Write
* DSR: Full Read and Write
* DS2: N/A
* DS3: N/A
* DeS: Partial Read and Write
* BB: N/A
* NB: Full Read and Write

### BND4
A general-purpose file container used from DS2 onwards.  
Extension: `.*bnd`
* DS1: N/A
* DSR: N/A
* DS2: Untested
* DS3: Full Read and Write
* DeS: N/A
* BB: Untested
* NB: N/A

### BXF3
Essentially a BND3 split into separate header and data files.  
Extensions: `.*bhd` (header) and `.*bdt` (data)
* DS1: Full Read and Write
* DSR: Full Read and Write
* DS2: N/A
* DS3: N/A
* DeS: Untested
* BB: N/A
* NB: N/A

### BXF4
Essentially a BND4 split into separate header and data files.  
Extensions: `.*bhd` (header) and `.*bdt` (data)
* DS1: N/A
* DSR: N/A
* DS2: Untested
* DS3: Full Read and Write
* DeS: N/A
* BB: Untested
* NB: N/A

### DCX
A wrapper for a single compressed file used in every game after NB.  
Extension: `.dcx`
* DS1: Full Read, Write, and Create
* DSR: Full Read, Write, and Create
* DS2: Full Read, Write, and Create
* DS3: Full Read, Write, and Create
* DeS: Full Read, Write, and Create
* BB: Full Read, Write, and Create
* NB: N/A

### DRB
An interface element configuration file used before DS2 when Scaleform was adopted. Very poorly supported.  
Extension: `.drb`
* DS1: Partial Read, No Write
* DSR: Partial Read, No Write
* DS2: N/A
* DS3: N/A
* DeS: Untested
* BB: N/A
* NB: Untested

### MTD
A material definition file used throughout the series.  
Extension: `.mtd`
* DS1: Full Read and Write
* DSR: Full Read and Write
* DS2: Untested
* DS3: Untested
* DeS: Untested
* BB: Untested
* NB: Untested

### TPF
A container for multiple DDS textures used throughout the series.  
Extension: `.tpf`
* DS1: Untested
* DSR: Full Read and Write
* DS2: Untested
* DS3: Untested
* DeS: No support
* BB: Untested
* NB: Full Read and Write

## Special Thanks
To everyone below, for either creating tools that I learned from, or helping decipher these formats one way or another. Please yell at me on Discord if I missed you.
* Atvaark
* B3LYP
* HotPocketRemix
* Meowmaritus
* Nyxojaele
* SeanP
* Wulf2k
