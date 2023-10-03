using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessManager : MonoSingleton<PostProcessManager>
{
    private Volume currentVolume;

    [SerializeField] private Volume[] volumes;

    private void Start()
    {
        currentVolume = volumes[0];
        currentVolume.weight = 1;
    }

    public async Task SwitchVolume(float fade, int volumeIndex)
    {
        Volume other = volumes[volumeIndex];
        float timer = 0;
        while (timer < fade)
        {
            currentVolume.weight = math.lerp(1, 0, timer / fade);
            other.weight = math.lerp(0, 1, timer / fade);
            await Task.Yield();
            timer += Time.deltaTime;
        }

        currentVolume.weight = 0;
        other.weight = 1;
        currentVolume = other;
    }
}
