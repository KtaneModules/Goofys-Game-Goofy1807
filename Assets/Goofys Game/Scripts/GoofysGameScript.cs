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
    public TextMesh Screen;
    public GameObject[] LEDs;
    public KMSelectable[] KeyPad;
    public KMSelectable Submit;
    public Material[] Materials;

    static int moduleIdCounter = 1;
    int moduleId;
    bool moduleSolved = false;
    bool Swapped = false;
    string Solution = "";
    string Input = "";
    List<int> lightCodes = new List<int>();

    KMSelectable.OnInteractHandler KeyPadPress(int btn)
    {
        return delegate
        {
            if (moduleSolved)
                return false;
            KeyPad[btn].AddInteractionPunch();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, KeyPad[btn].transform);
            if (btn == 10)
            {
                if (Input.Length == 0)
                    return false;
                else
                    Input = Input.Substring(0, Input.Length - 1);
            }
            else
            {
                if (Input.Length == 33)
                    return false;
                else
                    Input += btn.ToString();
            }
            setScreen(Input, 255, 255, 255);
            return false;
        };
    }

    void Start()
    {
        moduleId = moduleIdCounter++;

        for (int i = 0; i < KeyPad.Length; i++)
        {
            KeyPad[i].OnInteract += KeyPadPress(i);
        }

        Submit.OnInteract += delegate
        {
            if (moduleSolved)
                return false;

            if (Input.Equals(Solution))
            {
                Module.HandlePass();
                moduleSolved = true;
                setScreen("GG solved", 0, 255, 0);
                StopAllCoroutines();
                StartCoroutine(Victory());
            }
            else
            {
                Debug.LogFormat(@"[Goofy's Game #{0}] Incorrect - Your number was {1} - Expected number is {2}.", moduleId, Input, Solution);
                Module.HandleStrike();
                setScreen("Incorrect!", 255, 0, 0);
                Input = "";
            }

            return false;
        };

        for (int i = 0; i < 3; i++)
        {
            lightCodes.Add(Random.Range(0, 10));
            StartCoroutine(LightCodeEmitter(lightCodes[i], i));
        }

        var mS = Random.Range(0, 2);
        if (mS != 0)
        {
            Swapped = true;
            var pos = KeyPad[0].transform.localPosition;
            var rot = KeyPad[0].transform.localRotation;
            var sca = KeyPad[0].transform.localScale;
            KeyPad[0].transform.localPosition = KeyPad[10].transform.localPosition;
            KeyPad[0].transform.localRotation = KeyPad[10].transform.localRotation;
            KeyPad[0].transform.localScale = KeyPad[10].transform.localScale;
            KeyPad[10].transform.localPosition = pos;
            KeyPad[10].transform.localRotation = rot;
            KeyPad[10].transform.localScale = sca;
        }

        var snLetters = Bomb.GetSerialNumberLetters().Select(c => c - 'A' + 1).ToList();
        var snDigits = Bomb.GetSerialNumberNumbers().ToList();
        for (int i = 0; i < snDigits.Count; i++)
            snLetters.Add(snDigits[i]);
        var widgets = Bomb.GetBatteryHolderCount() + Bomb.GetIndicators().Count() + Bomb.GetPortPlateCount();

        if (!Swapped)
            Solution = ((lightCodes[0] + widgets) * (lightCodes[1] + snLetters.Sum()) + lightCodes[2] + widgets).ToString();
        else
            Solution = ((lightCodes[0] + snLetters.Sum()) * (lightCodes[1] + widgets) + lightCodes[2] + snLetters.Sum()).ToString();

        Debug.LogFormat(@"[Goofy's Game #{0}] DEL button is in the {1} position.", moduleId, Swapped ? "top left" : "bottom right");
        Debug.LogFormat(@"[Goofy's Game #{0}] Sum of serial number characters is {1}.", moduleId, snLetters.Sum().ToString());
        Debug.LogFormat(@"[Goofy's Game #{0}] Sum of battery holders, indicators and portplates is {1}.", moduleId, widgets.ToString());
        Debug.LogFormat(@"[Goofy's Game #{0}] The LEDs are morsing the following numbers: {1}", moduleId, lightCodes.Join(", ").ToString());
        Debug.LogFormat(@"[Goofy's Game #{0}] Plugging variables into the equation: ({1} + {2}) × ({3} + {4}) + {5} + {2}", moduleId, lightCodes[0], Swapped ? snLetters.Sum() : widgets, lightCodes[1], Swapped ? widgets : snLetters.Sum(), lightCodes[2]);
        Debug.LogFormat(@"[Goofy's Game #{0}] First step of equation: {1} × {2} + {3}", moduleId, lightCodes[0] + (Swapped ? snLetters.Sum() : widgets), lightCodes[1] + (Swapped ? widgets : snLetters.Sum()), lightCodes[2] + (Swapped ? snLetters.Sum() : widgets));
        Debug.LogFormat(@"[Goofy's Game #{0}] Last step of equation: {1} + {2}", moduleId, (lightCodes[0] + (Swapped ? snLetters.Sum() : widgets)) * (lightCodes[1] + (Swapped ? widgets : snLetters.Sum())), lightCodes[2] + (Swapped ? snLetters.Sum() : widgets));
        Debug.LogFormat(@"[Goofy's Game #{0}] Solution before any iterations is {1}.", moduleId, Solution);
        for (int i = 0; i < 4; i++)
        {
            Solution = conwaySequence(Solution);
            Debug.LogFormat(@"[Goofy's Game #{0}] Solution after {1} {2} is {3}.", moduleId, (i + 1).ToString(), i == 0 ? "iteration" : "iterations", Solution);
        }
    }

    void setScreen(string text, byte r, byte g, byte b)
    {
        Screen.text = text;
        Screen.color = new Color32(r, g, b, 255);
    }

    void SetLED(int i, bool j)
    {
        if (j)
            LEDs[i].GetComponent<MeshRenderer>().material = Materials[0];
        else
            LEDs[i].GetComponent<MeshRenderer>().material = Materials[1];

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

    IEnumerator Victory()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < 6; i++)
        {
            SetLED(0, true);
            yield return new WaitForSeconds(.05f);
            SetLED(1, true);
            yield return new WaitForSeconds(.05f);
            SetLED(2, true);
            SetLED(0, false);
            yield return new WaitForSeconds(.05f);
            SetLED(1, false);
            yield return new WaitForSeconds(.05f);
            SetLED(2, false);
            yield return new WaitForSeconds(.05f);
        }
        for (int i = 0; i < 6; i++)
        {
            SetLED(2, true);
            yield return new WaitForSeconds(.05f);
            SetLED(1, true);
            yield return new WaitForSeconds(.05f);
            SetLED(0, true);
            SetLED(2, false);
            yield return new WaitForSeconds(.05f);
            SetLED(1, false);
            yield return new WaitForSeconds(.05f);
            SetLED(0, false);
            yield return new WaitForSeconds(.05f);
        }
        for (int i = 0; i < 6; i++)
        {
            SetLED(2, true);
            SetLED(1, true);
            SetLED(0, true);
            yield return new WaitForSeconds(.1f);
            SetLED(2, false);
            SetLED(1, false);
            SetLED(0, false);
            yield return new WaitForSeconds(.1f);
        }
    }

    string conwaySequence(string number)
    {
        StringBuilder result = new StringBuilder();

        char repeat = number[0];
        number = number.Substring(1, number.Length - 1) + " ";
        int times = 1;

        foreach (char actual in number)
        {
            if (actual != repeat)
            {
                result.Append(Convert.ToString(times) + repeat);
                times = 1;
                repeat = actual;
            }
            else
            {
                times += 1;
            }
        }
        return result.ToString();
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
            if (num.Length + Input.Length > 33)
            {
                yield return "sendtochaterror Too many numbers. This would exceed the maximum of 33 digits on the screen";
                yield break;
            }
            for (int i = 0; i < num.Length; i++)
            {
                KeyPad[int.Parse(num[i].ToString())].OnInteract();
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
            if (num > Input.Length)
            {
                yield return "sendtochaterror Number is too big. It needs to be smaller than the amount of digits on the screen";
                yield break;
            }
            for (int i = 0; i < num; i++)
            {
                KeyPad[10].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
            yield break;
        }
        else if (Regex.IsMatch(command, @"^\s*(submit)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            Submit.OnInteract();
            yield break;
        }
        else
        {
            yield return "sendtochaterror Invalid Command";
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        Debug.LogFormat(@"[Goofy's Game #{0}] Module was force solved by TP.", moduleId);

        while (!moduleSolved)
        {

            while (Input.Length > Solution.Length)
            {
                KeyPad[10].OnInteract();
                yield return new WaitForSeconds(.1f);
            };

            var w = Solution.Substring(0, Input.Length);

            for (int i = w.Length - 1; i >= 0; i--)
            {
                if (w[i] != Input[i])
                {
                    KeyPad[10].OnInteract();
                    yield return new WaitForSeconds(.1f);
                }
            }

            var n = Solution.Substring(Input.Length);
            foreach (var digit in n)
            {
                KeyPad[int.Parse(digit.ToString())].OnInteract();
                yield return new WaitForSeconds(.1f);
            }

            if (Input.Length == Solution.Length)
                Submit.OnInteract();
            yield return new WaitForSeconds(.1f);
        }
    }
}