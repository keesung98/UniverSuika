using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Planet : MonoBehaviour
{
    public Gamemanager manager;
    public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach;
    public Rigidbody2D ridgid;
    CircleCollider2D circle;
    Animator anim;
    SpriteRenderer sprintRenderer;
    float daedTime;
    void Awake() 
    {
        ridgid=GetComponent<Rigidbody2D>();
        anim=GetComponent<Animator>();
        circle=GetComponent<CircleCollider2D>();
        sprintRenderer=GetComponent<SpriteRenderer>();
    }
    void OnEnable() 
    {
        anim.SetInteger("Level", level);
    }
    void OnDisable() 
    {
        level=0;
        isDrag=false;
        isMerge=false;
        isAttach=false;

        transform.localPosition=Vector3.zero;
        transform.localRotation=Quaternion.identity;
        transform.localScale=Vector3.zero;

        ridgid.simulated=false;
        ridgid.velocity=Vector2.zero;
        ridgid.angularVelocity=0;
        circle.enabled=true;
    }
    void Update()
    {
        if (isDrag)
        {
            Vector3 mousePos= Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float leftBorder=-4.7f+transform.localScale.x/2;
            float rightBorder=4.7f-transform.localScale.x/2;
            if (mousePos.x<leftBorder)
            {
                mousePos.x=leftBorder;
            }
            else if (mousePos.x>rightBorder)
            {
                mousePos.x=rightBorder;
            }
            mousePos.y=5.5f;
            mousePos.z=0;
            transform.position=Vector3.Lerp(transform.position,mousePos,0.1f);   
        }
    }

    public void Drag()
    {
        isDrag=true;
    }
    public void Drop()
    {
        isDrag=false;
        ridgid.simulated=true;
    }
    void OnCollisionEnter2D(Collision2D collision) 
    {
        StartCoroutine(AttachRoutine());
    }

    IEnumerator AttachRoutine()
    {
        if (isAttach)
        {
            yield break;
        }
        isAttach=true;
        manager.SfxPlay(Gamemanager.Sfx.Attach);
        yield return new WaitForSeconds(0.2f);
        isAttach=false;
    }
    void OnCollisionStay2D(Collision2D collision) 
    {
        if (collision.gameObject.tag=="Planets")
        {
            Planet other=collision.gameObject.GetComponent<Planet>();

            if (level==other.level && !isMerge&& !other.isMerge && level<7)
            {
                float meX=transform.position.x;
                float meY=transform.position.x;
                float otherX=other.transform.position.x;
                float otherY=other.transform.position.x;
                if (meY<otherY || (meY==otherY && meX>otherX))
                {
                    other.Hide(transform.position);
                    LevelUp();
                }
            }
        }
    }
    public void Hide(Vector3 targetPos)
    {
        isMerge=true;
        ridgid.simulated=false;
        circle.enabled=false;

        if (targetPos== Vector3.up * 100)
        {
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targetPos));
    }
    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount=0;
        while(frameCount<20)
        {
            frameCount++;
            if (targetPos != Vector3.up *100)
            {
                transform.position=Vector3.Lerp(transform.position,targetPos,0.5f);
            }
            else if(targetPos==Vector3.up*100)
            {
                transform.localScale=Vector3.Lerp(transform.localScale,Vector3.zero,0.2f);
            }
            yield return null;
        }

        manager.score+=(int)Mathf.Pow(2,level);

        isMerge=false;
        gameObject.SetActive(false);
    }
    void LevelUp()
    {
        isMerge=true;
        ridgid.velocity=Vector2.zero;
        ridgid.angularVelocity=0;

        StartCoroutine(LevelUpRoutine());
    }
    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        anim.SetInteger("Level",level+1);
        EffectPlay();
        manager.SfxPlay(Gamemanager.Sfx.Next);
        yield return new WaitForSeconds(0.3f);
        level++;

        manager.maxLevel=Mathf.Max(level,manager.maxLevel);
        if (manager.maxLevel>5)
        {
            manager.maxLevel=5;
        }

        isMerge=false;
    }

    void OnTriggerStay2D(Collider2D collision) 
    {
        if (collision.tag=="Finish")
        {
            daedTime+=Time.deltaTime;

            if (daedTime>2)
            {
                sprintRenderer.color=Color.red;
            }
            if (daedTime>5)
            {
                manager.GameOver();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision) 
    {
        if (collision.tag=="Finish")
        {
            daedTime=0;
            sprintRenderer.color=Color.white;
        }
    }
    void EffectPlay()
    {
        effect.transform.position=transform.position;
        effect.transform.localScale=2*transform.localScale;
        effect.Play();
    }
}
