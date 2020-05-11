using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public class GoofysGameScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public TextMesh S;
    public GameObject[] L;
    public KMSelectable[] KP;
    public KMSelectable Su;
    public Material[] Mat;

    static int moduleIdCounter = 1;
    int moduleId;
    bool moduleSolved = false;
    bool mode = false;
    string r;
    string inp = "";
    List<int> lC = new List<int>();

    KMSelectable.OnInteractHandler KPP(int btn)
    {
        return delegate
        {
            if (moduleSolved)
                return false;
            KP[btn].AddInteractionPunch();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, KP[btn].transform);
            if (btn == 10)
            {
                if (inp.Length == 0)
                    return false;
                else
                    inp = inp.Substring(0, inp.Length - 1);
            }
            else
            {
                if (inp.Length == 33)
                    return false;
                else
                    inp += btn.ToString();
            }
            setScreen(inp, 255, 255, 255);
            return false;
        };
    }

    void Start()
    {
        moduleId = moduleIdCounter++;

        for (int i = 0; i < KP.Length; i++)
        {
            KP[i].OnInteract += KPP(i);
        }

        Su.OnInteract += delegate
        {
            if (moduleSolved)
                return false;

            if (inp.Equals(r))
            {
                Module.HandlePass();
                moduleSolved = true;
                setScreen("GG solved", 0, 255, 0);
                StopAllCoroutines();
                StartCoroutine(Vic());
            }
            else
            {
                if (inp.Length > r.Length)
                {
                    if (Random.Range(0, 6) < 2)
                        Debug.LogFormat(@"[Goofy's Sequence #{0}] Incorrect - Your number has {1} more {2} than the solution", moduleId, (inp.Length - r.Length).ToString(), inp.Length - r.Length == 1 ? "digit" : "digits");
                    else
                        Debug.LogFormat(@"[Goofy's Game #{0}] Incorrect - Your number has {1} more {2} than the solution", moduleId, (inp.Length - r.Length).ToString(), inp.Length - r.Length == 1 ? "digit" : "digits");
                }
                else if (inp.Length < r.Length)
                {
                    if (Random.Range(0, 6) < 2)
                        Debug.LogFormat(@"[Conway's Game #{0}] Incorrect - Your number has {1} less {2} than the solution", moduleId, (r.Length - inp.Length).ToString(), r.Length - inp.Length == 1 ? "digit" : "digits");
                    else
                        Debug.LogFormat(@"[Goofy's Game #{0}] Incorrect - Your number has {1} less {2} than the solution", moduleId, (r.Length - inp.Length).ToString(), r.Length - inp.Length == 1 ? "digit" : "digits");
                }
                else
                    Debug.LogFormat(@"[Goofy's Game #{0}] Incorrect - Your number has the same amount of digits as the solution, but your digits are wrong.", moduleId);
                Module.HandleStrike();
                setScreen("Incorrect!", 255, 0, 0);
                inp = "";
            }

            return false;
        };

        for (int i = 0; i < 3; i++)
        {
            lC.Add(Random.Range(0, 10));
            StartCoroutine(LightCodeEmitter(lC[i], i));
        }

        var mS = Random.Range(0, 2);
        if (mS != 0)
        {
            mode = true;
            var pos = KP[0].transform.localPosition;
            var rot = KP[0].transform.localRotation;
            var sca = KP[0].transform.localScale;
            KP[0].transform.localPosition = KP[10].transform.localPosition;
            KP[0].transform.localRotation = KP[10].transform.localRotation;
            KP[0].transform.localScale = KP[10].transform.localScale;
            KP[10].transform.localPosition = pos;
            KP[10].transform.localRotation = rot;
            KP[10].transform.localScale = sca;
        }

        var s = Bomb.GetSerialNumberLetters().Select(c => c - 'A' + 1).ToList();
        var d = Bomb.GetSerialNumberNumbers().ToList();
        for (int i = 0; i < d.Count; i++)
            s.Add(d[i]);
        var w = Bomb.GetBatteryHolderCount() + Bomb.GetIndicators().Count() + Bomb.GetPortPlateCount();

        if (!mode)
            r = ((lC[0] + w) * (lC[1] + s.Sum()) + lC[2] + w).ToString();
        else
            r = ((lC[0] + s.Sum()) * (lC[1] + w) + lC[2] + s.Sum()).ToString();

        for (int i = 0; i < 4; i++)
        {
            r = doit(r);
        }
    }

    void setScreen(string text, byte r, byte g, byte b)
    {
        S.text = text;
        S.color = new Color32(r, g, b, 255);
    }

    void SetLED(int i, bool j)
    {
        if (j)
            L[i].GetComponent<MeshRenderer>().material = Mat[0];
        else
            L[i].GetComponent<MeshRenderer>().material = Mat[1];

    }

    IEnumerator LightCodeEmitter(int i, int j)
    {
        while (!moduleSolved)
        {
            if (i < 6 && i > 0)
            {
                var n = 5;
                for (int x = 0; x < i; x++)
                {
                    SetLED(j, true);
                    yield return new WaitForSeconds(0.2f);
                    SetLED(j, false);
                    yield return new WaitForSeconds(0.2f);
                    n--;
                }
                for (int x = 0; x < n; x++)
                {
                    SetLED(j, true);
                    yield return new WaitForSeconds(0.8f);
                    SetLED(j, false);
                    yield return new WaitForSeconds(0.2f);
                }
                yield return new WaitForSeconds(2f);
            }
            else
            {
                if (i == 0)
                    i = 5;
                else
                    i -= 5;
                var n = 5;
                for (int x = 0; x < i; x++)
                {
                    SetLED(j, true);
                    yield return new WaitForSeconds(0.8f);
                    SetLED(j, false);
                    yield return new WaitForSeconds(0.2f);
                    n--;
                }
                for (int x = 0; x < n; x++)
                {
                    SetLED(j, true);
                    yield return new WaitForSeconds(0.2f);
                    SetLED(j, false);
                    yield return new WaitForSeconds(0.2f);
                }
                if (i == 5)
                    i = 0;
                else
                    i += 5;
                yield return new WaitForSeconds(2f);
            }
        }
    }

    IEnumerator Vic()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 4; i++)
        {
            SetLED(0, true);
            yield return new WaitForSeconds(.1f);
            SetLED(1, true);
            yield return new WaitForSeconds(.1f);
            SetLED(2, true);
            SetLED(0, false);
            yield return new WaitForSeconds(.1f);
            SetLED(1, false);
            yield return new WaitForSeconds(.1f);
            SetLED(2, false);
            yield return new WaitForSeconds(.1f);
        }
        for (int i = 0; i < 4; i++)
        {
            SetLED(2, true);
            yield return new WaitForSeconds(.1f);
            SetLED(1, true);
            yield return new WaitForSeconds(.1f);
            SetLED(0, true);
            SetLED(2, false);
            yield return new WaitForSeconds(.1f);
            SetLED(1, false);
            yield return new WaitForSeconds(.1f);
            SetLED(0, false);
            yield return new WaitForSeconds(.1f);
        }
        for (int i = 0; i < 4; i++)
        {
            SetLED(2, true);
            SetLED(1, true);
            SetLED(0, true);
            yield return new WaitForSeconds(.2f);
            SetLED(2, false);
            SetLED(1, false);
            SetLED(0, false);
            yield return new WaitForSeconds(.2f);
        }
    }

    string doit(string n)
    {
        StringBuilder r = new StringBuilder();

        char re = n[0];
        n = n.Substring(1, n.Length - 1) + " ";
        int times = 1;

        foreach (char a in n)
        {
            if (a != re)
            {
                r.Append(Convert.ToString(times) + re);
                times = 1;
                re = a;
            }
            else
            {
                times += 1;
            }
        }
        return r.ToString();
    }

#pragma warning disable 0414
    readonly string TwitchHelpMessage = "!{0} 12345 [input 12345] | !{0} submit [press submit] | !{0} del 12 [press delete 12 times]";
#pragma warning restore 0414

    IEnumerator ProcessTwitchCommand(string command)
    {
        Match m;
        if (moduleSolved)
        {
            yield return "sendtochaterror The module is already solved";
            yield break;
        }
        else if ((m = Regex.Match(command, @"^\s*(?<num>[1234567890]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            yield return null;
            var num = m.Groups["num"].Value;
            if (num.Length + inp.Length > 33)
            {
                yield return "sendtochaterror Too many numbers. This would exceed the maximum of 33 digits on the screen";
                yield break;
            }
            for (int i = 0; i < num.Length; i++)
            {
                KP[int.Parse(num[i].ToString())].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
            yield break;

        }
        else if ((m = Regex.Match(command, @"^\s*(del)\s+(?<num>[1234567890]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            yield return null;
            var num = int.Parse(m.Groups["num"].Value);
            if (num == 0)
            {
                yield return "sendtochaterror You are trying to delete 0 digits. Good Job.";
                yield break;
            }
            if (num > inp.Length)
            {
                yield return "sendtochaterror Number is too big. It needs to be smaller than the amount of digits on the screen";
                yield break;
            }
            for (int i = 0; i < num; i++)
            {
                KP[10].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
            yield break;
        }
        else if (Regex.IsMatch(command, @"^\s*(submit)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            Su.OnInteract();
            yield break;
        }
        else
        {
            yield return "sendtochaterror Invalid Command";
            yield break;
        }
    }
}