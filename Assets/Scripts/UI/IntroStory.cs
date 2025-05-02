using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntroStory : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI storyText;
    private string[] story;
    [SerializeField] SceneChange sceneChanger;
    private int currentIndex;
    // Start is called before the first frame update
    void Start()
    {
        story = new string[8];
        story[0] = "Every duck is told about this place when they are young.";
        story[1] = "Most believe it to be a fairytale, others believe it truly exists.";
        story[2] = "A collection of universes; alternate versions of each other existing simultaneously in peace, unaware of what lies beyond their own...";
        story[3] = "THE QUACKADOX.";
        story[4] = "If left alone, the universes will remain in tact and all will follow the destined path...";
        story[5] = "But someone, or something, has disturbed the peace; causing the universes to collide.";
        story[6] = "A figure appears to you in a dream and tells you that you are the only hope to restore the balance.";
        story[7] = "Now armed with the ability to create portals and traverse the Quackadox, you set out on your journey!";

        storyText.SetText(story[0]);
        currentIndex = 0;
    }

    public void NextPage()
    {
        if (currentIndex < story.Length - 1)
        {
            storyText.SetText(story[currentIndex + 1]);
            currentIndex++;
        }
        else
        {
            sceneChanger.LoadScene("SampleScene");
        } 
            
    }
}
