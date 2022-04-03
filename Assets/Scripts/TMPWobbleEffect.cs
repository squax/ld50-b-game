using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using System;
using UnityEngine.Scripting;

public class TMPWobbleEffect : MonoBehaviour
{
    public enum BringInEffect
    {
        Wobble,
        Cinematic
    }

    TMP_Text textMesh;
    TextMeshPro textMeshPro;

    Mesh mesh;

    List<Vector3> vertices = new List<Vector3>();
    List<Color> colours = new List<Color>();

    [SerializeField]
    private BringInEffect bringInEffect = BringInEffect.Wobble;

    [SerializeField]
    private bool typeWriterEnabled = false;

    [SerializeField]
    private float typeWriterSpeed = 0.2f;

    [SerializeField]
    [Range(0f, 1f)]
    private float targetAlpha = 1.0f;

    [SerializeField]
    private bool animateColor = true;

    [SerializeField]
    private bool useColourGradient = false;

    [SerializeField]
    private Color gradFrom;

    [SerializeField]
    private Color gradTo;

    [SerializeField]
    private Vector2 wobble = new Vector2(1f, 1f);

    [SerializeField]
    private Vector2 wobbleScale = new Vector2(1f, 1f);

    [SerializeField]
    private AnimationCurve wobbleCurve;

    [SerializeField]
    private float effectTime = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    private float effectTimeOverride = 0f;

    [SerializeField]
    private bool overrideTime = false;

    [SerializeField]
    private float effectDelay = 0;

    [SerializeField]
    private Vector2 wobbleIdle = new Vector2(1f, 1f);

    [SerializeField]
    [Range(0f, 1f)]
    private float faceSoftness = 0f;

    [SerializeField]
    private string[] typeWriterSounds;

    private float runningTime = 0;

    private bool runColourOnce = false;

    private float cachedWidth = 0f;

    private bool reverseEffect = false;

    private float typeWriterCurrentTime = 0;

    private int currentVisibleCharacters = 0;

    private int characterLength = 0;

    private float refreshRate = 0.01666f;
    private float runTime = 0f;

    private AudioSource audioSource;

    private bool isPaused = false;

    public bool IsPaused { set { isPaused = value;  } }

    [SerializeField]
    private float masterAlpha = 1f;

    public float MaserAlpha { set { masterAlpha = value; } }

    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
        textMeshPro = GetComponent<TextMeshPro>();
        audioSource = GetComponent<AudioSource>();

        if (textMesh != null)
        {
            textMesh.ForceMeshUpdate();
        }
        else if (textMeshPro != null)
        {
            textMeshPro.ForceMeshUpdate();
        }

        RestartTime();

        Update();
    }

    [ContextMenu("Restart Effect Time")]
    public void RestartTime()
    {
        runningTime = 0;
        runColourOnce = false;
        reverseEffect = false;

        if(typeWriterEnabled == true)
        {
            typeWriterCurrentTime = 0;
            currentVisibleCharacters = 0;

            if (textMesh != null)
            {
                textMesh.maxVisibleCharacters = currentVisibleCharacters;
            }

            else if (textMeshPro != null)
            {
                textMeshPro.maxVisibleCharacters = currentVisibleCharacters;
            }
        }

        if (textMeshPro != null)
        {
            SetText(textMeshPro.text);
        }

        else if (textMesh != null)
        {
            SetText(textMesh.text);
        }

    }

    public void ReverseEffect()
    {
        runningTime = effectTime + effectDelay;
        reverseEffect = true;
    }

    public int TotalVisibleCharacters
    {
        get
        {
            if(textMeshPro != null)
            {
                return textMeshPro.textInfo.characterCount;
            }

            return textMesh.textInfo.characterCount;
        }
    }

    public void SetText(string text)
    {
        if(textMesh != null)
        {
            textMesh.text = text;

            textMesh.ForceMeshUpdate(true, true);

            characterLength = textMesh.textInfo.characterCount;
        }

        else if (textMeshPro != null)
        {
            textMeshPro.text = text;

            textMeshPro.ForceMeshUpdate(true, true);

            characterLength = textMeshPro.textInfo.characterCount;
        }
    }

    void Update()
    {
        if(isPaused == true)
        {
            return;
        }

        if (typeWriterEnabled == true && currentVisibleCharacters < characterLength)
        {
            bool playSoundsOnce = true;

            typeWriterCurrentTime += Time.unscaledDeltaTime;

            float speed = 1f / typeWriterSpeed;

            while (typeWriterCurrentTime >= speed)
            {
                typeWriterCurrentTime = typeWriterCurrentTime - speed;

                ++currentVisibleCharacters;

                if(currentVisibleCharacters >= characterLength)
                {
                    currentVisibleCharacters = characterLength;
                }
                else
                {
                    if(playSoundsOnce == true && audioSource != null && typeWriterSounds != null && typeWriterSounds.Length > 0)
                    {
                        playSoundsOnce = false;
                        AudioManager.Instance.PlayOneShot(typeWriterSounds[UnityEngine.Random.Range(0, typeWriterSounds.Length)], audioSource, 1f);
                    }
                }

                if (textMesh != null)
                {
                    textMesh.maxVisibleCharacters = currentVisibleCharacters;
                }

                else if (textMeshPro != null)
                {
                    textMeshPro.maxVisibleCharacters = currentVisibleCharacters;
                }
            }
        }

        if (reverseEffect == true)
        {
            runningTime -= Time.unscaledDeltaTime;

            if(runningTime < 0f)
            {
                runningTime = 0f;
            }
        }
        else
        {
            runningTime += Time.unscaledDeltaTime;
        }

        runTime += Time.unscaledDeltaTime;

        if(runTime <= refreshRate)
        {
            //return;
        }

        runTime = 0f;

        var actualTime = runningTime - effectDelay;

        if(actualTime < 0)
        {
            actualTime = 0f;
        }

        if (textMesh != null)
        {
            textMesh.ForceMeshUpdate();
            mesh = textMesh.mesh;
        }
        else if (textMeshPro != null)
        {
            textMeshPro.ForceMeshUpdate();
            mesh = textMesh.mesh;
        }

        mesh.GetVertices(vertices);
        mesh.GetColors(colours);

        float perComplete = overrideTime ? effectTimeOverride : Mathf.Clamp01(actualTime / effectTime);

        float wobbleMod = 1.0f;
        if (wobbleCurve != null)
        {
            wobbleMod = wobbleCurve.Evaluate(perComplete);
        }

        TMP_TextInfo textInfo = null;

        if (textMesh != null)
        {
            textInfo = textMesh.textInfo;
        }
        else if (textMeshPro != null)
        {
            textInfo = textMeshPro.textInfo;
        }

        if (perComplete < 1f)
        {
            // Modify verts.
            for (int i = 0; i < textInfo.characterCount; ++i)
            {
                var charInfo = textInfo.characterInfo[i];

                int index = charInfo.vertexIndex;

                switch(bringInEffect)
                {
                    case BringInEffect.Wobble:
                        Vector3 offset = Wobble(Time.unscaledTime + i, wobble.x, wobble.y, wobbleScale.x * wobbleMod, wobbleScale.y * wobbleMod);

                        vertices[index] += offset;
                        vertices[index + 1] += offset;
                        vertices[index + 2] += offset;
                        vertices[index + 3] += offset;
                        break;

                    case BringInEffect.Cinematic:
                        offset = Wobble(Time.unscaledTime + i, wobble.x, wobble.y, wobbleScale.x * wobbleMod, wobbleScale.y * wobbleMod);

                        vertices[index] += offset;
                        vertices[index + 1] += offset;
                        vertices[index + 2] += offset;
                        vertices[index + 3] += offset;

                        vertices[index + 2] *= perComplete;
                        vertices[index + 3] *= perComplete;
                        break;
                }

                if (animateColor == true)
                {
                    var newColor = colours[index];
                    newColor.a = perComplete * targetAlpha * masterAlpha;

                    if (useColourGradient == true)
                    {
                        gradFrom.a = gradTo.a = newColor.a;

                        colours[index] = gradFrom;
                        colours[index + 1] = gradTo;
                        colours[index + 2] = gradTo;
                        colours[index + 3] = gradFrom;
                    }
                    else
                    {
                        colours[index] = newColor;
                        colours[index + 1] = newColor;
                        colours[index + 2] = newColor;
                        colours[index + 3] = newColor;
                    }
                }
            }
        }
        else
        {
            for(int i = 0; i < textInfo.characterCount; ++i)
            {
                var charInfo = textInfo.characterInfo[i];

                int index = charInfo.vertexIndex;

                Vector3 offset = Wobble(Time.unscaledTime + i, wobbleIdle.x, wobbleIdle.y, 1f, 1f);

                vertices[index] += offset;
                vertices[index + 1] += offset;
                vertices[index + 2] += offset;
                vertices[index + 3] += offset;

                if (animateColor == true)
                {
                    var newColor = colours[index];
                    newColor.a = perComplete * targetAlpha * masterAlpha;

                    if (useColourGradient == true)
                    {
                        gradFrom.a = gradTo.a = newColor.a;

                        colours[index] = gradFrom;
                        colours[index + 1] = gradTo;
                        colours[index + 2] = gradTo;
                        colours[index + 3] = gradFrom;
                    }
                    else
                    {
                        colours[index] = newColor;
                        colours[index + 1] = newColor;
                        colours[index + 2] = newColor;
                        colours[index + 3] = newColor;
                    }
                }
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetColors(colours);

        if (textMesh != null && textMesh.canvasRenderer != null)
        {
            textMesh.canvasRenderer.SetMesh(mesh);
        }

        /*
        if (useColourGradient == true)
        {
            if (textMesh != null)
            {
                textMesh.colorGradient = new VertexGradient(gradFrom, gradFrom, gradTo, gradTo);

                textMesh.SetAllDirty();
            }

            if (textMeshPro != null)
            {
                textMeshPro.colorGradient = new VertexGradient(gradFrom, gradFrom, gradTo, gradTo);

                textMeshPro.SetAllDirty();
            }
        }
        */
    }

    private Vector2 Wobble(float time, float xScale, float yScale, float xWobbleScale, float yWobbleScale)
    {
        return new Vector2(Mathf.Sin(time * xScale) * xWobbleScale, Mathf.Cos(time * yScale) * yWobbleScale);
    }
}
