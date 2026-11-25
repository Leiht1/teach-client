using UnityEngine;

namespace IslandSystem
{
    public sealed class BeachSurvival : MonoBehaviour
    {
        public Transform driftwood;
        public Transform wreckCrate;
        public Transform salvageBarrel;
        public Light hearth;
        public GameObject embers;
        public Transform[] stones;
        public Transform shelter, wreckMarker, exit;
        public int Stone { get; private set; }
        public int GatheredWood { get; private set; }
        public bool CampVisited { get; private set; }
        public bool CampChecked { get; private set; }
        public bool WreckChecked { get; private set; }
        public bool Searched { get; private set; }
        public int Stage { get; private set; }
        public event System.Action<Vector3> PickedUp;
        public float LastProgress { get; private set; }
        public bool BagOpen { get; set; }
        public string Loot { get; private set; }
        public float LootTime { get; private set; }
        public float StageTime { get; private set; }
        public Transform Focus => focus;
        public BeachChapter Chapter { get; private set; }
        void Awake() => Chapter=GetComponent<BeachChapter>();
        Vector3 start;
        float startYaw;
        bool initialized;
        public bool Moved { get; private set; }
        public bool Looked { get; private set; }

        public int Wood { get; private set; }
        public bool Lit => hearth.enabled;
        public bool FireStarted { get; private set; }

        ActionMotor motor;
        float fireUntil;
        Transform focus;

        void Update()
        {
            if (Lit && Time.time >= fireUntil)
                SetFire(false);

            if (!motor)
                motor = FindAnyObjectByType<ActionMotor>();

            focus = null;
            if (!motor || BagOpen || motor.FreeCameraActive || motor.InputBlocked || motor.Busy ||
                (motor.view && !motor.view.isActiveAndEnabled))
                return;

            if (Chapter && Chapter.ManualLocked) return;
            if (!Chapter) Advance(motor.transform.position, motor.Yaw);
            RefreshFocus(motor.transform.position);
            if (focus && Input.GetKeyDown(KeyCode.F))
                Use(motor.transform.position, focus);
        }

        public bool TryInteract(ActionMotor actor)
        {
            var target = FindFocus(actor.transform.position);
            if ((Chapter && Chapter.ManualLocked) || !target || BagOpen || actor.InputBlocked || actor.Busy || actor.FreeCameraActive)
                return false;
            Use(actor.transform.position, target);
            return true;
        }

        public void Advance(Vector3 position, float yaw)
        {
            if (!initialized)
            {
                initialized = true;
                start = position;
                startYaw = yaw;
                LastProgress = StageTime = Time.unscaledTime;
            }
            var horizontal = position - start;
            horizontal.y = 0;
            Moved |= horizontal.sqrMagnitude >= 4;
            Looked |= Mathf.Abs(Mathf.DeltaAngle(startYaw,yaw)) >= 15;
            if (Stage >= 2)
                CampVisited |= Vector3.Distance(position,hearth.transform.position) < 4;
            bool complete = Stage switch
            {
                0 => Moved && Looked,
                1 => GatheredWood >= 3 && Stone >= 2,
                2 => CampVisited && CampChecked,
                3 => Searched && WreckChecked,
                4 => exit && Vector3.Distance(position,exit.position) < 5,
                _ => false
            };
            if (complete)
            {
                Stage++;
                LastProgress = StageTime = Time.unscaledTime;
            }
        }

        public Transform HintTarget(Vector3 player, float now)
        {
            if (now - LastProgress < 22f) return null;
            return Stage switch
            {
                1 when GatheredWood < 3 => Nearest(new[] { driftwood, salvageBarrel, wreckCrate }),
                1 when Stone < 2 => Nearest(stones),
                2 => !CampVisited ? hearth.transform : shelter,
                3 => WreckChecked ? Nearest(new[] { wreckCrate, salvageBarrel }) : wreckMarker,
                4 => exit,
                _ => null
            };

            Transform Nearest(Transform[] options)
            {
                Transform best = null;
                float distance = float.PositiveInfinity;
                foreach (var option in options ?? System.Array.Empty<Transform>())
                {
                    if (!option || !option.gameObject.activeInHierarchy) continue;
                    float next = (option.position-player).sqrMagnitude;
                    if (next >= distance) continue;
                    distance = next;
                    best = option;
                }
                return best;
            }
        }

        public Transform NearestWood(Vector3 player)
        {
            Transform nearest = null;
            float distance = float.PositiveInfinity;
            foreach (var target in new[] { driftwood, wreckCrate, salvageBarrel })
            {
                if (!target || !target.gameObject.activeInHierarchy) continue;
                float next = (target.position-player).sqrMagnitude;
                if (next >= distance) continue;
                nearest = target;
                distance = next;
            }
            return nearest;
        }

        public void RefreshFocus(Vector3 position) => focus = FindFocus(position);

        public Transform FindFocus(Vector3 position)
        {
            Transform best = null;
            float range = 2.25f;
            var priority = Chapter ? Chapter.PriorityCampTarget : null;
            Consider(priority);
            if (best) return best;
            Consider(driftwood);
            Consider(wreckCrate);
            Consider(salvageBarrel);
            Consider(hearth.transform);
            Consider(shelter);
            Consider(wreckMarker);
            if (Chapter && Chapter.Step==9) Consider(Chapter.supplies);
            foreach (var stone in stones ?? System.Array.Empty<Transform>())
                Consider(stone);
            return best;

            void Consider(Transform target)
            {
                if (!target || !target.gameObject.activeInHierarchy ||
                    (target == shelter && CampChecked) || (target == wreckMarker && WreckChecked))
                    return;
                float distance = Vector3.Distance(position,target.position);
                if (distance < range)
                {
                    range = distance;
                    best = target;
                }
            }
        }

        public string Describe(Transform target) => Chapter && target == Chapter.supplies ? "搜索补给箱" : target == hearth.transform
            ? Lit ? "营火燃烧中" : Wood > 0 ? "点燃营火" : "营火需要木材"
            : target == shelter ? "检查布棚"
            : target == wreckMarker ? "检查沉船残骸"
            : target == driftwood ? "拾取漂流木"
            : target == wreckCrate ? "搜索木箱"
            : target == salvageBarrel ? "搜索木桶" : "捡起石头";

        void Acquired(string name, int count, Vector3 position)
        {
            Loot = name + "  ×" + count;
            LastProgress = LootTime = Time.unscaledTime;
            PickedUp?.Invoke(position);
        }

        public bool Use(Vector3 playerPosition, Transform target)
        {
            if (!target || !target.gameObject.activeInHierarchy ||
                Vector3.Distance(playerPosition, target.position) >= 2.25f)
                return false;

            if (Chapter && target == Chapter.supplies && Chapter.TakeSupplies(playerPosition))
            {
                Wood+=3;
                GatheredWood+=3;
                Acquired("木材",3,target.position);
                return true;
            }
            if (target == hearth.transform)
            {
                if (Wood == 0 || Lit)
                    return false;
                Wood--;
                FireStarted = true;
                fireUntil = Time.time + 180f;
                SetFire(true);
                return true;
            }

            if (target == shelter && !CampChecked) { CampChecked = true; LastProgress = Time.unscaledTime; return true; }
            if (target == wreckMarker && !WreckChecked) { WreckChecked = true; LastProgress = Time.unscaledTime; return true; }
            if (stones != null && System.Array.IndexOf(stones, target) >= 0)
            {
                Stone++;
                Acquired("石头",1,target.position);
                target.gameObject.SetActive(false);
                return true;
            }

            int amount = target == wreckCrate ? 3 : target == salvageBarrel ? 2 : target == driftwood ? 1 : 0;
            if (amount == 0)
                return false;
            Wood += amount;
            GatheredWood += amount;
            Searched |= target == wreckCrate || target == salvageBarrel;
            Acquired("木材",amount,target.position);
            target.gameObject.SetActive(false);
            return true;
        }

        void SetFire(bool lit)
        {
            hearth.enabled = lit;
            embers.SetActive(lit);
        }

    }
}
