using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Mandelbrot : MonoBehaviour
{
    public ComputeShader mandelbrotShader;
    public RenderTexture renderTexture;
    
    // public float xOffset = -1.74999841099374081749002483162428393452822172335808534616943930976364725846655540417646727085571962736578151132907961927190726789896685696750162524460775546580822744596887978637416593715319388030232414667046419863755743802804780843375f;
    // public float yOffset = -0.00000000000000165712469295418692325810961981279189026504290127375760405334498110850956047368308707050735960323397389547038231194872482690340369921750514146922400928554011996123112902000856666847088788158433995358406779259404221904755f;
    public float xOffset = 0f;
    public float yOffset = 0f;
    private int width = 1920;
    private int height = 1080;
    
    public float scroll = 2.0f;
    public float rValReal = 0f;
    public float rValImag = 0f;
    public bool JuliaSet = false;
    public bool FileWriting = false;
    private uint iterations = 10000;
    
    private bool scrolling = false;
    private uint frames = 0;
    private float movementSens = 0.1f;
    private float scrollSens = 0.9f;
    private float rSens = 0.1f;

    private List<float> red = new List<float>();
    private List<float> blue = new List<float>();
    private List<float> green = new List<float>();
    
    private ComputeBuffer redB;
    private ComputeBuffer greenB;
    private ComputeBuffer blueB;
    
    private Texture2D shaderValues; 
    private Rect region;


    void Start() 
    {
        shaderValues = new Texture2D(width, height, TextureFormat.RGBA32, false);

        ComputeBuffer redB = new ComputeBuffer(1000, sizeof(float));
        ComputeBuffer greenB = new ComputeBuffer(1000, sizeof(float));
        ComputeBuffer blueB = new ComputeBuffer(1000, sizeof(float));

        foreach (string line in File.ReadLines("Assets/red.txt"))
        {
            red.Add(float.Parse(line));
        }
        foreach (string line in File.ReadLines("Assets/green.txt"))
        {
            green.Add(float.Parse(line));
        }
        foreach (string line in File.ReadLines("Assets/blue.txt"))
        {
            blue.Add(float.Parse(line));
        }

        redB.SetData(red);
        greenB.SetData(green);
        blueB.SetData(blue);

        mandelbrotShader.SetBuffer(0, "rColours", redB);
        mandelbrotShader.SetBuffer(0, "gColours", greenB);
        mandelbrotShader.SetBuffer(0, "bColours", blueB);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (renderTexture == null) 
        {
            renderTexture = new RenderTexture(width, height, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        mandelbrotShader.SetTexture(0, "Result", renderTexture);
        mandelbrotShader.SetFloat("ResolutionX", renderTexture.width);
        mandelbrotShader.SetFloat("ResolutionY", renderTexture.height);
        mandelbrotShader.SetFloat("ColourCount", red.Count);
        mandelbrotShader.SetFloat("xOffset", xOffset);
        mandelbrotShader.SetFloat("yOffset", yOffset);
        mandelbrotShader.SetFloat("scroll", scroll);
        mandelbrotShader.SetFloat("Iterations", iterations);
        mandelbrotShader.SetFloat("RReal", rValReal);
        mandelbrotShader.SetFloat("RImag", rValImag);
        mandelbrotShader.SetBool("Julia", JuliaSet);
        mandelbrotShader.Dispatch(0, renderTexture.width/8, renderTexture.height/8, 1);
        
        region = new Rect(0, 0, width, height);
        RenderTexture.active = renderTexture;
        shaderValues.ReadPixels(region, 0, 0, false);

        if (FileWriting)
        {
            byte[] bytes = shaderValues.EncodeToPNG();

            File.WriteAllBytes(@$"C:\Users\Ed\Documents\Edmund\Images\mandelbrot\mandelbrot{frames}.png", bytes);
        }

        Graphics.Blit(renderTexture, dest);
        
        frames += 1;

        if (scrolling)
        {
            scroll *= 0.9f;
        }
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown("j"))
        {
            JuliaSet = JuliaSet ? false : true;
        } else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            yOffset += (movementSens * scroll);
        } else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            yOffset -= (movementSens * scroll);
        } else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            xOffset -= (movementSens * scroll);
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            xOffset += (movementSens * scroll);
        } else if (Input.GetKeyDown(KeyCode.Minus))
        {
            scroll *= scrollSens;
        } else if (Input.GetKeyDown(KeyCode.Equals))
        {
            scroll /= scrollSens;
        } else if (Input.GetKeyDown("w"))
        {
            rValReal += rSens; 
        } else if (Input.GetKeyDown("s"))
        {
            rValReal -= rSens;
        } else if (Input.GetKeyDown("a"))
        {
            rValImag -= rSens;
        } else if (Input.GetKeyDown("d"))
        {
            rValImag += rSens;
        }
    }
}

// ffmpeg -f image2 -r 20 -i mandelbrot%01d.png -vcodec gif -y output.gif 