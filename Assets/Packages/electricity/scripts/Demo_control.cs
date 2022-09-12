using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo_control : MonoBehaviour
{
    public Material[] materials;
    public Renderer renderer;

    public Text text_title;
    public string[] titles;

    public AudioSource audio_source;
    public AudioClip ka;

    private int index = 0;


    void Start()
    {
        this.index = 0;

        this.renderer.material = this.materials[this.index];

        this.text_title.text = this.titles[this.index];

        this.audio_source.PlayOneShot(this.ka, 0.66f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.index++;
            if (this.index >= this.materials.Length)
                this.index = 0;

            this.renderer.material = this.materials[this.index];

            this.text_title.text = this.titles[this.index];

            this.audio_source.PlayOneShot(this.ka, 0.66f);
        }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.index--;
            if (this.index <= -1)
                this.index = (this.materials.Length - 1);

            this.renderer.material = this.materials[this.index];

            this.text_title.text = this.titles[this.index];

            this.audio_source.PlayOneShot(this.ka, 0.66f);
        }
    }

}
