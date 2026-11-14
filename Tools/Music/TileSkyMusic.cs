using System;
using System.IO;

// Original, deterministic compositions. Render outside Unity with RenderMusic.ps1.
public static class TileSkyMusic
{
    const int Rate = 44100;
    static double[] left, right;
    static int length;
    static double beat;
    static readonly int[][] Chords = {
        new[] {48, 55, 59, 62, 64}, new[] {47, 55, 57, 62, 67},
        new[] {45, 52, 55, 59, 60}, new[] {41, 48, 52, 55, 57},
        new[] {50, 53, 57, 60, 64}, new[] {40, 47, 50, 55, 59},
        new[] {41, 48, 52, 57, 60}, new[] {43, 50, 55, 57, 60}
    };
    static readonly int[][] Melody = {
        new[] {76, 79, 83, 79, 74, 76}, new[] {74, 79, 81, 79, 74, 71},
        new[] {72, 76, 79, 83, 79, 76}, new[] {76, 72, 69, 72, 79, 76},
        new[] {74, 77, 81, 79, 77, 74}, new[] {71, 74, 79, 76, 74, 71},
        new[] {72, 76, 81, 79, 76, 72}, new[] {74, 79, 81, 79, 74, 71}
    };
    static double Frequency(int note) { return 440 * Math.Pow(2, (note - 69) / 12.0); }
    static void Add(int sample, double value, double pan)
    {
        int index = sample % length;
        left[index] += value * Math.Sqrt((1 - pan) * .5);
        right[index] += value * Math.Sqrt((1 + pan) * .5);
    }
    // Piano-like struck strings, a soft bell, warm pad and rounded bass.
    static void Note(double at, int note, double duration, double volume, double pan, int voice)
    {
        int start = (int)(at * beat * Rate);
        int count = (int)(duration * beat * Rate);
        double f = Frequency(note), seconds = duration * beat;
        for (int i = 0; i < count; i++)
        {
            double t = (double)i / Rate, p = 2 * Math.PI * f * t;
            double envelope, tone;
            if (voice == 0)
            {
                envelope = (1 - Math.Exp(-t * 230)) * Math.Exp(-t * 2.2)
                    * Math.Min(1, (seconds - t) / .12);
                tone = Math.Sin(p) + .28 * Math.Sin(p * 2.001) * Math.Exp(-t * 2)
                    + .12 * Math.Sin(p * 3.004) * Math.Exp(-t * 4)
                    + .045 * Math.Sin(p * 5.01) * Math.Exp(-t * 6);
            }
            else if (voice == 1)
            {
                envelope = (1 - Math.Exp(-t * 120)) * Math.Exp(-t * 1.8)
                    * Math.Min(1, (seconds - t) / .15);
                tone = Math.Sin(p + 1.8 * Math.Sin(p * 2) * Math.Exp(-t * 6))
                    + .13 * Math.Sin(p * 3) * Math.Exp(-t * 3);
            }
            else if (voice == 2)
            {
                envelope = Math.Pow(Math.Sin(Math.PI * t / seconds), 1.5);
                tone = .55 * Math.Sin(p) + .22 * Math.Sin(p * 1.0015)
                    + .14 * Math.Sin(p * 2) + .07 * Math.Sin(p * 3);
            }
            else
            {
                envelope = (1 - Math.Exp(-t * 100)) * Math.Exp(-t * 2.8)
                    * Math.Min(1, (seconds - t) / .08);
                tone = Math.Sin(p) + .18 * Math.Sin(p * 2);
            }
            Add(start + i, tone * envelope * volume, pan);
        }
    }
    static void Drum(double at, int type, double volume, Random rng)
    {
        double duration = type == 0 ? .24 : .13;
        int start = (int)(at * beat * Rate);
        double previous = 0;
        for (int i = 0; i < (int)(duration * Rate); i++)
        {
            double t = (double)i / Rate;
            double noise = rng.NextDouble() * 2 - 1;
            double high = (noise - previous) * .5;
            previous = noise;
            double sample = type == 0
                ? Math.Sin(2 * Math.PI * (48 * t + 1.8 * (1 - Math.Exp(-t * 30))))
                    * Math.Exp(-t * 22) * (1 - Math.Exp(-t * 800))
                : high * Math.Exp(-t * (type == 1 ? 65 : 38))
                    * (1 - Math.Exp(-t * 1500));
            Add(start + i, sample * volume, type == 0 ? 0 : .3);
        }
    }
    public static void Render(string path, bool game)
    {
        beat = 60.0 / (game ? 104 : 88);
        length = (int)Math.Round(32 * 4 * beat * Rate);
        left = new double[length]; right = new double[length];
        Random rng = new Random(game ? 104 : 88);
        for (int bar = 0; bar < 32; bar++)
        {
            int c = bar % 8, section = bar / 8;
            double at = bar * 4;
            int[] chord = Chords[c];
            // Slow, overlapping pads retain air underneath the rhythmic figures.
            for (int j = 1; j < chord.Length; j++)
                Note(at, chord[j], 5.5, game ? .022 : .032, (j - 2.5) * .35, 2);
            Note(at, chord[0] - 12, 2.5, game ? .17 : .12, 0, 3);
            Note(at + 2.5, chord[0] - 12, 1.35, game ? .11 : .065, 0, 3);
            int[] pattern = {1, 2, 3, 2, 4, 2, 3, 2};
            int steps = game ? 8 : 4;
            for (int j = 0; j < steps; j++)
            {
                int n = chord[pattern[j]] + (game ? 12 : 0);
                Note(at + j * (game ? .5 : 1) + (j % 2 == 1 ? .035 : 0), n,
                    2.6, (game ? .072 : .095) * (.88 + rng.NextDouble() * .2),
                    j % 2 == 0 ? -.35 : .35, 0);
            }
            // Four phrases share a motif; the middle section leaves more space.
            double[] offsets = game ? new[] {0.0, .75, 1.5, 2.5, 3.0, 3.5}
                                    : new[] {0.0, .5, 1.5, 2.5, 3.0, 3.5};
            for (int j = 0; j < 6; j++)
            {
                if ((section == 2 && j % 2 == 1) || (!game && j == 1)) continue;
                int n = Melody[c][j];
                if (section == 1 && j == 2) n += 12;
                Note(at + offsets[j], n, 2.8, (game ? .105 : .092)
                    * (j == 0 ? 1 : .8), -.12, game ? 0 : 1);
            }
            if (section == 1 || section == 3)
                Note(at + 2, chord[3] + 24, 3.5, .035, .55, 1);
            if (game)
            {
                Drum(at, 0, .14, rng); Drum(at + 2, 0, .115, rng);
                Drum(at + 1, 2, .055, rng); Drum(at + 3, 2, .05, rng);
                for (int j = 0; j < 8; j++)
                    Drum(at + j * .5 + (j % 2) * .035, 1, j % 2 == 0 ? .027 : .018, rng);
            }
        }
        // Circular stereo room taps preserve every tail across the loop boundary.
        double[] dryL = (double[])left.Clone(), dryR = (double[])right.Clone();
        double[] delays = {.113, .179, .293, .431, .619};
        double[] gains = {.12, .10, .075, .05, .03};
        for (int tap = 0; tap < delays.Length; tap++)
        {
            int delay = (int)(delays[tap] * Rate);
            for (int i = 0; i < length; i++)
            {
                int src = (i - delay + length) % length;
                left[i] += (tap % 2 == 0 ? dryR[src] : dryL[src]) * gains[tap];
                right[i] += (tap % 2 == 0 ? dryL[src] : dryR[src]) * gains[tap];
            }
        }
        double peak = 0, sum = 0;
        for (int i = 0; i < length; i++)
        {
            peak = Math.Max(peak, Math.Max(Math.Abs(left[i]), Math.Abs(right[i])));
            sum += left[i] * left[i] + right[i] * right[i];
        }
        double rms = Math.Sqrt(sum / (length * 2));
        double gain = Math.Min(.88 / peak, .14 / rms);
        using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + length * 4);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
            writer.Write((short)1); writer.Write((short)2); writer.Write(Rate);
            writer.Write(Rate * 4); writer.Write((short)4); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(length * 4);
            for (int i = 0; i < length; i++)
            {
                writer.Write((short)Math.Round(left[i] * gain * 32767));
                writer.Write((short)Math.Round(right[i] * gain * 32767));
            }
        }
        Console.WriteLine("{0}: {1:F2}s, peak {2:F3}, RMS {3:F3}, seam L/R {4:F5}/{5:F5}",
            Path.GetFileName(path), (double)length / Rate, peak * gain, rms * gain,
            Math.Abs(left[0] - left[length - 1]) * gain,
            Math.Abs(right[0] - right[length - 1]) * gain);
        left = right = null;
    }
}
