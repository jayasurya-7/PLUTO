// using UnityEngine;
// using System.Collections.Generic;
// using System.Collections;

// public class SeedHighlighter : MonoBehaviour
// {
//     public List<SeedController> seeds;
//     private SeedController currentHighlighted;

//     private float rainTimer = 0f;
//     private float highlightTimer = 0f;

//     private float rainDurationToGrow = 0.02f;    // needs 1s of rain
//     private float highlightDuration = 4.5f;     // highlighted for 3s
//     private float gapDuration = 0.2f;         // 0.5s gap between highlights

//     private bool inGap = false;
//     private bool hasGrownThisCycle = false;
//     private GameManager gm;
//     private int currentTarget = 0;

//     void Start()
//     {
//         gm = FindObjectOfType<GameManager>();
//         HighlightRandomSeed();
//     }

//     void Update()
//     {
//         if (gm != null && gm.enabled == false) return; // stop if game over
//         if (inGap) return;
//         if (currentHighlighted == null) return;

//         highlightTimer += Time.deltaTime;
//         if ((highlightTimer >= highlightDuration)||hasGrownThisCycle)
//         {
//             StartCoroutine(SwitchAfterGap());
//             return;
//         }

//         // Growth logic
//         if (!hasGrownThisCycle)
//         {
//             if (currentHighlighted.IsBeingRainedOn)
//             {
//                 rainTimer += Time.deltaTime;
//                 if (rainTimer >= rainDurationToGrow)
//                 {
//                     currentHighlighted.Grow();
//                     hasGrownThisCycle = true;
//                     gm.score++;
//                 }
//             }
//             else
//             {
//                 rainTimer = 0f;
//             }
//         }
//     }

//     IEnumerator SwitchAfterGap()
//     {
//          inGap = true;
//         if (currentHighlighted != null)
//             currentHighlighted.SetHighlight(false);

//         currentHighlighted = null;
//         foreach (var seed in seeds)
//             seed.IsBeingRainedOn = false;

//         yield return new WaitForSeconds(gapDuration);

//         HighlightRandomSeed();
//         highlightTimer = 0f;
//         rainTimer = 0f;
//         hasGrownThisCycle = false;
//          inGap = false;
//     }

//     // public void HighlightRandomSeed()
//     // {
//     //     foreach (var seed in seeds)
//     //     {
//     //         seed.SetHighlight(false);
//     //         seed.highLighter.SetActive(false);
//     //     }
//     //     var available = seeds.FindAll(s => !s.IsFullyGrown);
//     //     if (gm.score < gm.totalTargets)
//     //     {
//     //         if (available.Count == 0)
//     //         {
//     //             currentHighlighted = null;
//     //             Debug.Log("✅ All seeds fully grown!");
//     //             return;
//     //         }

//     //         int index = Random.Range(0, available.Count);
//     //         currentHighlighted = available[index];
//     //         currentHighlighted.SetHighlight(true);
//     //         Debug.Log($"🌟 Highlighted: {currentHighlighted.name}");
//     //     }
//     //     else
//     //     {
//     //         currentHighlighted = null;
//     //             Debug.Log("✅ Game Ended");
//     //             return;
//     //     }

//     // }
//     private SeedController lastHighlighted = null; // store last seed

// public void HighlightRandomSeed()
// {
//     foreach (var seed in seeds)
//     {
//         seed.SetHighlight(false);
//         seed.highLighter.SetActive(false);
//     }

//     var available = seeds.FindAll(s => !s.IsFullyGrown);

//     if (gm.score < gm.totalTargets)
//     {
//         if (available.Count == 0)
//         {
//             currentHighlighted = null;
//             Debug.Log(" All seeds fully grown!");
//                 gm.EndGame();
//             return;
//         }

//         // exclude last highlighted if possible
//         if (lastHighlighted != null && available.Count > 1)
//         {
//             available.Remove(lastHighlighted);
//         }

//         int index = Random.Range(0, available.Count);
//         currentHighlighted = available[index];
//         currentHighlighted.SetHighlight(true);

//         // remember for next call
//         lastHighlighted = currentHighlighted;

//         Debug.Log($"Highlighted: {currentHighlighted.name}");
//     }
//     else
//     {
//         currentHighlighted = null;
//                 gm.EndGame();
//         Debug.Log(" Game Ended");
//     }
// }


//     public bool IsHighlighted(SeedController seed) => seed == currentHighlighted;
// }
