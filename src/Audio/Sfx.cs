using System;
using System.Collections.Generic;
using Godot;

namespace SpiritOfMurin.Audio;

/// <summary>
/// Procedural sound, like the HTML prototype's WebAudio sfx: short tones and filtered noise bursts
/// synthesised into an <see cref="AudioStreamGenerator"/>, plus an ambient bed (wind and birds) that
/// can fade in and out. <see cref="Master"/> fades everything (the silence of the prologue's ending).
/// No audio files: nothing to license, nothing to load.
/// </summary>
public partial class Sfx : Node
{
    private const int Rate = 22050;

    private sealed class Voice
    {
        public float T, Dur, F0, F1, Vol, Delay, Lp, Q;
        public bool Noise;
        public int Wave; // 0 square, 1 sine, 2 triangle, 3 saw
        public float Phase, Lo, Band;
    }

    private AudioStreamPlayer _player = null!;
    private AudioStreamGeneratorPlayback? _pb;
    private readonly List<Voice> _voices = new();
    private uint _rng = 12345;
    private float _wind, _windTarget, _birds, _birdsTarget, _windLo, _windGust, _birdTimer = 1.5f;

    /// <summary>Master volume 0-1 (fades with <see cref="FadeMaster"/>).</summary>
    public float Master { get; private set; } = 1f;
    private float _masterTarget = 1f, _masterSpeed = 1f;

    /// <summary>Overall loudness.</summary>
    [Export] public float Volume { get; set; } = 0.5f;

    /// <summary>Names of the sounds played (newest last), for checks.</summary>
    public List<string> Played { get; } = new();

    /// <inheritdoc />
    public override void _Ready()
    {
        _player = new AudioStreamPlayer { Stream = new AudioStreamGenerator { MixRate = Rate, BufferLength = 0.12f }, Name = "SfxPlayer" };
        AddChild(_player);
        _player.Play();
        _pb = _player.GetStreamPlayback() as AudioStreamGeneratorPlayback;
    }

    private float Rnd() { _rng ^= _rng << 13; _rng ^= _rng >> 17; _rng ^= _rng << 5; return (_rng & 0xFFFFFF) / 16777216f; }

    private void Tone(float f, float dur, int wave = 0, float vol = 0.3f, float delay = 0, float fEnd = 0) =>
        _voices.Add(new Voice { F0 = f, F1 = fEnd > 0 ? fEnd : f, Dur = dur, Wave = wave, Vol = vol, Delay = delay });

    private void Noise(float dur, float vol, float freq = 1200, float q = 1, float delay = 0) =>
        _voices.Add(new Voice { Noise = true, Dur = dur, Vol = vol, Lp = freq, Q = q, Delay = delay });

    /// <summary>Fades the master volume to <paramref name="target"/> over <paramref name="seconds"/>.</summary>
    public void FadeMaster(float target, float seconds) { _masterTarget = target; _masterSpeed = seconds <= 0 ? 100 : Mathf.Abs(target - Master) / seconds; }

    /// <summary>Sets the ambient bed: wind and birds, 0-1 each (they fade toward it).</summary>
    public void Ambient(float wind, float birds) { _windTarget = wind; _birdsTarget = birds; }

    /// <summary>Plays a named sound (the prototype's sfx names plus the ending's).</summary>
    public void Play(string n)
    {
        Played.Add(n); if (Played.Count > 80) Played.RemoveAt(0);
        switch (n)
        {
            case "swing": Noise(0.07f, 0.18f, 3200, 1.5f); break;
            case "swing3": Noise(0.11f, 0.24f, 2400, 1.2f); break;
            case "hit": Tone(160, 0.09f, 0, 0.32f, 0, 70); Noise(0.06f, 0.28f, 1800); break;
            case "parry": Tone(1180, 0.08f, 3, 0.18f); Tone(1760, 0.06f, 0, 0.12f, 0.01f); Noise(0.05f, 0.2f, 5200, 3); break;
            case "stone": Tone(220, 0.1f, 2, 0.2f, 0, 160); Noise(0.06f, 0.2f, 900); break;
            case "hurt": Tone(220, 0.16f, 0, 0.3f, 0, 90); Noise(0.12f, 0.25f, 900); break;
            case "warn": Tone(330, 0.06f, 0, 0.12f); Tone(330, 0.06f, 0, 0.12f, 0.09f); break;
            case "ki": Tone(440, 0.18f, 1, 0.2f, 0, 880); Noise(0.12f, 0.1f, 5000, 2); break;
            case "side": Noise(0.09f, 0.16f, 6000, 2); Tone(660, 0.08f, 1, 0.1f, 0, 990); break;
            case "step": Noise(0.05f, 0.1f, 1500, 1); break;
            case "evade": Tone(990, 0.08f, 1, 0.14f, 0, 1480); break;
            case "pickup": Tone(880, 0.06f, 0, 0.14f); Tone(1320, 0.08f, 0, 0.12f, 0.05f); break;
            case "chime": Tone(1046, 0.25f, 1, 0.2f); Tone(1568, 0.3f, 1, 0.14f, 0.07f); break;
            case "learn": Tone(523, 0.3f, 1, 0.2f); Tone(659, 0.3f, 1, 0.18f, 0.12f); Tone(784, 0.5f, 1, 0.18f, 0.24f); Tone(1046, 0.6f, 1, 0.15f, 0.4f); break;
            case "combo": Tone(392, 0.2f, 3, 0.18f, 0, 784); Tone(784, 0.4f, 1, 0.2f, 0.12f); Noise(0.2f, 0.12f, 6000, 2, 0.1f); break;
            case "gate": Tone(90, 0.4f, 3, 0.25f, 0, 60); Noise(0.4f, 0.2f, 600); break;
            case "page": Noise(0.12f, 0.12f, 4000, 2); break;
            case "pot": Noise(0.12f, 0.3f, 2200, 1.4f); Tone(600, 0.05f, 2, 0.12f); break;
            case "fizzle": Tone(200, 0.12f, 0, 0.12f, 0, 120); break;
            case "win": Tone(392, 0.25f, 1, 0.2f); Tone(523, 0.25f, 1, 0.2f, 0.15f); Tone(784, 0.6f, 1, 0.2f, 0.3f); break;
            case "die": Tone(330, 0.6f, 2, 0.24f, 0, 110); break;
            // the ending
            case "rumble": Noise(1.6f, 0.3f, 180, 0.8f); Tone(55, 1.6f, 1, 0.25f, 0, 70); break;
            case "charge": Tone(110, 1.8f, 3, 0.16f, 0, 440); Noise(1.8f, 0.14f, 900, 1.2f); break;
            case "explode": Noise(1.4f, 0.75f, 400, 0.7f); Tone(70, 1.2f, 0, 0.5f, 0, 30); Noise(0.4f, 0.6f, 3000, 1, 0.02f); break;
            case "heart": Tone(60, 0.12f, 1, 0.4f); Tone(55, 0.14f, 1, 0.32f, 0.22f); break;
            case "breath": Noise(0.9f, 0.08f, 900, 0.8f); break;
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        Master = Mathf.MoveToward(Master, _masterTarget, _masterSpeed * dt);
        _wind = Mathf.MoveToward(_wind, _windTarget, dt * 0.4f);
        _birds = Mathf.MoveToward(_birds, _birdsTarget, dt * 0.3f);
        if (_birds > 0.05f)
        {
            _birdTimer -= dt;
            if (_birdTimer <= 0)
            {
                _birdTimer = 0.8f + Rnd() * 2.6f;
                float f = 2200 + Rnd() * 1600, v = 0.05f * _birds;
                int notes = 2 + (int)(Rnd() * 3);
                for (int i = 0; i < notes; i++) Tone(f * (1 + Rnd() * 0.2f), 0.05f + Rnd() * 0.04f, 1, v, i * 0.09f, f * (1.2f + Rnd() * 0.3f));
            }
        }
        if (_pb == null) return;
        int frames = _pb.GetFramesAvailable();
        for (int i = 0; i < frames; i++)
        {
            float s = Sample(1f / Rate) * Volume * Master;
            s = Mathf.Clamp(s, -1, 1);
            _pb.PushFrame(new Vector2(s, s));
        }
    }

    private float Sample(float dt)
    {
        float sum = 0;
        for (int i = _voices.Count - 1; i >= 0; i--)
        {
            var v = _voices[i];
            if (v.Delay > 0) { v.Delay -= dt; continue; }
            v.T += dt;
            if (v.T >= v.Dur) { _voices.RemoveAt(i); continue; }
            float k = v.T / v.Dur, env = Mathf.Min(1, v.T / 0.004f) * (1 - k) * (1 - k);
            float x;
            if (v.Noise)
            {
                float n = Rnd() * 2 - 1, a = Mathf.Clamp(v.Lp / Rate * 6.28f, 0.001f, 1);
                v.Lo += (n - v.Lo) * a; // low-pass
                v.Band += (v.Lo - v.Band) * a * 0.5f;
                x = v.Q > 1.2f ? (v.Lo - v.Band) * v.Q : v.Lo;
            }
            else
            {
                float f = Mathf.Lerp(v.F0, v.F1, k);
                v.Phase = (v.Phase + f * dt) % 1f;
                x = v.Wave switch { 1 => Mathf.Sin(v.Phase * Mathf.Tau), 2 => 1 - 4 * Mathf.Abs(v.Phase - 0.5f), 3 => v.Phase * 2 - 1, _ => v.Phase < 0.5f ? 1 : -1 } * 0.5f;
            }
            sum += x * v.Vol * env;
        }
        if (_wind > 0.01f)
        {
            _windGust += dt * 0.35f;
            float g = 0.6f + 0.4f * Mathf.Sin(_windGust) * Mathf.Sin(_windGust * 0.37f);
            float n = Rnd() * 2 - 1; _windLo += (n - _windLo) * 0.02f;
            sum += _windLo * 0.9f * _wind * g;
        }
        return sum;
    }
}
