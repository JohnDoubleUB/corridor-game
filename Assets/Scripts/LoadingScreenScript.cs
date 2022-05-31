using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenScript : MonoBehaviour
{
    public MaskableGraphic[] CompanyLogo;
    public Image ProgressBar;

    // Start is called before the first frame update
    void Start()
    {
        DoAllTheThings();
    }


    private async void DoAllTheThings() 
    {
        await ShowCompanySplashScreen();
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync() 
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(LoadingData.sceneToLoad);
        operation.allowSceneActivation = false;

        while (!operation.isDone) 
        {
            ProgressBar.fillAmount = operation.progress;
            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }


    private async Task ShowCompanySplashScreen() 
    {
        if (CompanyLogo.Any()) 
        {
            List<Task> logoTasks = new List<Task>();


            foreach (MaskableGraphic graphic in CompanyLogo) 
            {
                logoTasks.Add(graphic.FadeMaskableGraphic(true));
            }

            await Task.WhenAll(logoTasks);
            await Task.Delay(System.TimeSpan.FromSeconds(2));
            logoTasks.Clear();

            foreach (MaskableGraphic graphic in CompanyLogo)
            {
                logoTasks.Add(graphic.FadeMaskableGraphic(false));
            }

            await Task.WhenAll(logoTasks);
        }
    }
}


public class LoadingData 
{
    public static string sceneToLoad = "GameplayScene";
}