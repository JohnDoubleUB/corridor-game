using UnityEngine;

public class MouseBob : MonoBehaviour
{
    public float walkingBobbingSpeed = 14f;
    public float bobbingAmount = 0.05f;
    public MouseEntity mouseEntity;
    public bool AllowMouseBob = true;

    public AudioSource mouseFootstepSource;
    private float defaultMouseFootstepVolume;
    private float volumeMultiplier;

    public AudioClip[] mouseWalkingSounds;

    float timer = 0;
    float defaultPosY = 0;

    // Start is called before the first frame update
    void Start()
    {
        defaultPosY = transform.position.y;
        if (mouseFootstepSource != null) defaultMouseFootstepVolume = mouseFootstepSource.volume;
    }

    // Update is called once per frame
    void Update()
    {
        if (AllowMouseBob && mouseEntity.MovementAmount > 0.001f)
        {
            //Player is moving
            timer += Time.deltaTime * walkingBobbingSpeed;
            transform.localPosition = new Vector3(transform.localPosition.x, defaultPosY + Mathf.Sin(timer) * bobbingAmount, transform.localPosition.z);
            if (!mouseFootstepSource.isPlaying)
            {
                mouseFootstepSource.clip = mouseWalkingSounds[Random.Range(0, mouseWalkingSounds.Length)];
                mouseFootstepSource.Play();
            }
            else 
            {
                if (volumeMultiplier != AudioManager.current.SoundVolumeMultiplier) 
                {
                    volumeMultiplier = AudioManager.current.SoundVolumeMultiplier;
                    mouseFootstepSource.volume = defaultMouseFootstepVolume * volumeMultiplier;
                }
            }
        }
        else
        {
            //Idle
            timer = 0;
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed), transform.localPosition.z);
            if (mouseFootstepSource.isPlaying) mouseFootstepSource.Stop();
        }
    }
}
