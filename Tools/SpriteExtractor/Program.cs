using SkiaSharp;
using System.Diagnostics;

// ═══════════════════════════════════════════════
// Tibia .spr Sprite Extractor
// Versão compatível: 10.41+ (uint32 sprite count)
// Formato: 32×32 pixels, RLE BGRA
// ═══════════════════════════════════════════════

const int SpriteSize = 32;
const int SpritePixels = SpriteSize * SpriteSize;

// ── Argumentos ──
string sprPath = args.Length > 0
    ? args[0]
    : @"C:\Users\PICHAU\Git\Tibia77\client\data\things\1041\Tibia.spr";

string outputDir = args.Length > 1
    ? args[1]
    : @"C:\Users\PICHAU\Git\GameDev\Client\Assets\Sprites";

if (!File.Exists(sprPath))
{
    Console.Error.WriteLine($"Arquivo não encontrado: {sprPath}");
    return 1;
}

Directory.CreateDirectory(outputDir);

Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║   Tibia .spr → PNG Extractor         ║");
Console.WriteLine("╚══════════════════════════════════════╝");
Console.WriteLine($"  Input:  {sprPath}");
Console.WriteLine($"  Output: {outputDir}");
Console.WriteLine();

var sw = Stopwatch.StartNew();

using var fs = File.OpenRead(sprPath);
using var reader = new BinaryReader(fs);

// Header: 4 bytes signature + 4 bytes sprite count (uint32 for v10.41+)
uint signature = reader.ReadUInt32();
uint spriteCount = reader.ReadUInt32();

Console.WriteLine($"  Signature:    0x{signature:X8}");
Console.WriteLine($"  Sprite count: {spriteCount:N0}");
Console.WriteLine();

// Lê todos os offsets (uint32 cada)
uint[] offsets = new uint[spriteCount + 1]; // +1 porque índice 0 não é usado
for (int i = 1; i <= spriteCount; i++)
{
    offsets[i] = reader.ReadUInt32();
}

int exported = 0;
int skipped = 0;

for (int id = 1; id <= spriteCount; id++)
{
    uint offset = offsets[id];

    // Offset 0 = sprite vazio
    if (offset == 0)
    {
        skipped++;
        continue;
    }

    // Proteção: offset fora do arquivo
    if (offset + 5 >= (uint)fs.Length)
    {
        skipped++;
        continue;
    }

    fs.Seek(offset, SeekOrigin.Begin);

    // 3 bytes: RGB color key (transparência) — ignoramos
    reader.ReadByte(); // R
    reader.ReadByte(); // G
    reader.ReadByte(); // B

    // 2 bytes: tamanho dos dados de pixel comprimidos
    ushort pixelDataSize = reader.ReadUInt16();

    if (pixelDataSize == 0)
    {
        skipped++;
        continue;
    }

    // Decodifica RLE → BGRA pixels
    byte[] pixels = new byte[SpritePixels * 4]; // BGRA, inicializado como transparente (0,0,0,0)

    int pixelIndex = 0;
    int readBytes = 0;
    bool corrupt = false;

    try
    {
        while (readBytes < pixelDataSize && pixelIndex < SpritePixels)
        {
            // Transparent pixels count
            ushort transparentCount = reader.ReadUInt16();
            readBytes += 2;

            // Colored pixels count
            ushort coloredCount = reader.ReadUInt16();
            readBytes += 2;

            pixelIndex += transparentCount; // pula transparentes (já são 0)

            for (int j = 0; j < coloredCount && pixelIndex < SpritePixels; j++)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                byte a = reader.ReadByte();
                readBytes += 4;

                int offset4 = pixelIndex * 4;
                pixels[offset4 + 0] = b;     // B
                pixels[offset4 + 1] = g;     // G
                pixels[offset4 + 2] = r;     // R
                pixels[offset4 + 3] = a;     // A

                pixelIndex++;
            }
        }
    }
    catch (EndOfStreamException)
    {
        corrupt = true;
    }

    if (corrupt) { skipped++; continue; }

    // Salva como PNG usando SkiaSharp
    using var bitmap = new SKBitmap(SpriteSize, SpriteSize, SKColorType.Bgra8888, SKAlphaType.Unpremul);
    var pinnedPixels = bitmap.GetPixels();
    System.Runtime.InteropServices.Marshal.Copy(pixels, 0, pinnedPixels, pixels.Length);

    var outputPath = Path.Combine(outputDir, $"{id}.png");
    using var image = SKImage.FromBitmap(bitmap);
    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
    using var outStream = File.OpenWrite(outputPath);
    data.SaveTo(outStream);

    exported++;

    if (exported % 1000 == 0)
        Console.Write($"\r  Exportando... {exported:N0} sprites");
}

sw.Stop();
Console.WriteLine($"\r  Concluído! {exported:N0} sprites exportados, {skipped:N0} vazios ignorados.");
Console.WriteLine($"  Tempo: {sw.Elapsed.TotalSeconds:F1}s");
Console.WriteLine($"  Pasta: {outputDir}");

return 0;
