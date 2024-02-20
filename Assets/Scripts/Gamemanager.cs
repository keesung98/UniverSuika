using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Gamemanager : MonoBehaviour
{
    [Header("-------------[ Core ]")]
    public bool isOver;
    public int score; 
    public int maxLevel;

    [Header("-------------[ Object Pooling ]")]
    public Planet lastPlanet;
    public GameObject planetPrefab;
    public Transform planetGroup;
    public List<Planet> planetPool;
    [Range(1,30)]
    public int poolSize;
    public int poolCursor;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Header("-------------[ Audoio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp,Next, Attach,Button,Over};
    int sfxCursor;

    [Header("-------------[ UI ]")]
    public Text scoreText;
    public Text maxScoreText;
    public Text planetNameText;
    public Text scoreTapText;
    public Text maxScoreTapText;
    public Text planetNameTapText;
    public GameObject endGroup;
    public GameObject startGroup;
    public float targetAspectRatio = 9f / 16f;

    [Header("-------------[ ETC ]")]
    public GameObject line;
    public GameObject top;



    
    void Awake() 
    {
        Application.targetFrameRate=60;

        planetPool=new List<Planet>();
        effectPool=new List<ParticleSystem>();
        for (int i = 0; i < poolSize; i++)
        {
            MakePlanet();
        }
        if (!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxtScore",0);
        }
        maxScoreText.text=PlayerPrefs.GetInt("MaxScore").ToString();
    }
    public void GameStart()
    {
        AdjustCameraSize();
        line.SetActive(true);
        top.SetActive(true);
        scoreTapText.gameObject.SetActive(true);
        maxScoreTapText.gameObject.SetActive(true);
        planetNameTapText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        planetNameText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(Sfx.Button);
        Invoke("NextPlanet", 1.5f);
    }

    void AdjustCameraSize()
    {
        float currentAspectRatio = (float)Screen.width / Screen.height;
        Camera.main.orthographicSize *= targetAspectRatio / currentAspectRatio;
    }

    Planet MakePlanet()
    {
        GameObject instantEffectObj=Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name="Effect "+effectPool.Count;
        ParticleSystem instantEffect=instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        GameObject instantPlanetObj=Instantiate(planetPrefab, planetGroup);
        instantPlanetObj.name="Planet "+planetPool.Count;
        Planet instantPlanet=instantPlanetObj.GetComponent<Planet>();
        instantPlanet.manager=this;
        instantPlanet.effect=instantEffect;
        planetPool.Add(instantPlanet);
        
        return instantPlanet;
    }
    Planet GetPlanet()
    {
        for (int i = 0; i < planetPool.Count; i++)
        {
            poolCursor=(poolCursor+1)%planetPool.Count;
            if (!planetPool[poolCursor].gameObject.activeSelf)
            {
                return planetPool[poolCursor];
            }
        }
        return MakePlanet();
    }
    void NextPlanet()
    {
        if (isOver)
        {
            return;
        }

        lastPlanet=GetPlanet();
        lastPlanet.level=Random.Range(0,maxLevel);
        lastPlanet.gameObject.SetActive(true);

        switch (lastPlanet.level)
        {
            case(0):
            planetNameText.text="Moon";
            break;
            case(1):
            planetNameText.text="Mercury";
            break;
            case(2):
            planetNameText.text="Mars";
            break;
            case(3):
            planetNameText.text="Venus";
            break;
            case(4):
            planetNameText.text="Earth";
            break;
        }

        SfxPlay(Sfx.Next);
        StartCoroutine(WaitNext());
    }
    IEnumerator WaitNext()
    {
        while (lastPlanet!=null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);
        NextPlanet();
    }
    public void TouchDown()
    {
        if (lastPlanet==null)
        {
            return;
        }
        lastPlanet.Drag();
    }
    public void TouchUp()
    {
        if (lastPlanet==null)
        {
            return;
        }
        lastPlanet.Drop();
        lastPlanet=null;
    }
    public void GameOver()
    {
        if (isOver)
        {
            return;
        }
        isOver=true;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        Planet[] planets=FindObjectsOfType<Planet>();

        for (int i = 0; i < planets.Length; i++)
        {
            planets[i].ridgid.simulated=false;
        }
        for (int i = 0; i < planets.Length; i++)
        {
            planets[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        int maxScore=Mathf.Max(score,PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore",maxScore);

        endGroup.SetActive(true);
        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine(ResetCoroutine());
    }
    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }

    public void SfxPlay(Sfx type)
    {
        switch(type)
        {
            case Sfx.LevelUp: 
            sfxPlayer[sfxCursor].clip=sfxClip[0];
            break;
            case Sfx.Next: 
            sfxPlayer[sfxCursor].clip=sfxClip[1];
            break;
            case Sfx.Attach: 
            sfxPlayer[sfxCursor].clip=sfxClip[2];
            break;
            case Sfx.Button: 
            sfxPlayer[sfxCursor].clip=sfxClip[3];
            break;
            case Sfx.Over: 
            sfxPlayer[sfxCursor].clip=sfxClip[4];
            break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor=(sfxCursor+1)%sfxPlayer.Length;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }
    void LateUpdate() 
    {
        scoreText.text=score.ToString();
    }
}
