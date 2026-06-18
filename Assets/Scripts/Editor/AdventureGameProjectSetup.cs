using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public static class AdventureGameProjectSetup
{
    private const string ScenePath = "Assets/Scenes/AdventureGame.unity";

    private struct SpriteSet
    {
        public Sprite Player;
        public Sprite Enemy;
        public Sprite Npc;
        public Sprite Projectile;
        public Sprite Pickup;
        public Sprite Grass;
        public Sprite Wall;
        public Sprite Hazard;
        public Sprite Crate;
        public Sprite Flower;
    }

    private struct TileSet
    {
        public Tile Grass;
        public Tile Wall;
    }

    private struct ParticleSet
    {
        public ParticleSystem Repair;
        public ParticleSystem Pickup;
    }

    [MenuItem("Tools/2D Beginner Adventure/Rebuild Course Project")]
    public static void RebuildCourseProject()
    {
        EnsureFolders();
        ConfigureProject();

        SpriteSet sprites = CreateSprites();
        TileSet tiles = CreateTiles(sprites);
        ParticleSet particles = CreateParticles();
        AnimatorController playerAnimator = CreatePlayerAnimator();
        AnimatorController enemyAnimator = CreateEnemyAnimator();
        GameObject projectilePrefab = CreateProjectilePrefab(sprites.Projectile, particles.Repair);
        GameObject pickupPrefab = CreatePickupPrefab(sprites.Pickup, particles.Pickup);
        GameObject enemyPrefab = CreateEnemyPrefab(sprites.Enemy, enemyAnimator, particles.Repair);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateWorld(tiles, sprites);
        GameObject player = CreatePlayer(sprites.Player, playerAnimator, projectilePrefab);
        MalfunctioningEnemy[] enemies = CreateEnemies(enemyPrefab);
        CreatePickups(pickupPrefab);
        DialogueActor npc = CreateNpc(sprites.Npc);
        CreateHud(player.GetComponent<Health>());
        CreateCamera(player.transform);
        CreateObjectiveTracker(enemies, npc);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ValidateCourseProject();
        Debug.Log("[2D Beginner Adventure] Course project rebuilt.");
    }

    [MenuItem("Tools/2D Beginner Adventure/Validate Course Project")]
    public static void ValidateCourseProject()
    {
        RequireAsset<SceneAsset>(ScenePath);
        RequireAsset<VisualTreeAsset>("Assets/UI/Hud.uxml");
        RequireAsset<StyleSheet>("Assets/UI/Hud.uss");
        RequireAsset<GameObject>("Assets/Prefabs/RepairProjectile.prefab");
        RequireAsset<GameObject>("Assets/Prefabs/MalfunctioningEnemy.prefab");
        RequireAsset<GameObject>("Assets/Prefabs/HealthCollectible.prefab");

        if (EditorBuildSettings.scenes.Length == 0 || EditorBuildSettings.scenes[0].path != ScenePath)
        {
            throw new InvalidOperationException("AdventureGame scene is not registered in build settings.");
        }

        Debug.Log("[2D Beginner Adventure] Validation passed.");
    }

    private static void EnsureFolders()
    {
        string[] folders =
        {
            "Assets/Scenes",
            "Assets/Sprites",
            "Assets/Tiles",
            "Assets/Prefabs",
            "Assets/Animations",
            "Assets/UI"
        };

        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/") ?? "Assets";
                string name = Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    private static void ConfigureProject()
    {
        PlayerSettings.companyName = "UnityLearnPractice";
        PlayerSettings.productName = "2D Beginner Adventure Game";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;

        var property = typeof(PlayerSettings).GetProperty("activeInputHandling");
        if (property != null)
        {
            object both = Enum.Parse(property.PropertyType, "Both");
            property.SetValue(null, both);
        }
    }

    private static SpriteSet CreateSprites()
    {
        return new SpriteSet
        {
            Player = LoadSpriteOrCreate("Assets/Art/Ruby/PlayerCharacter.png", "Player", new Color(0.18f, 0.55f, 0.95f, 1f), new Color(0.95f, 0.98f, 1f, 1f), SpritePattern.Character),
            Enemy = LoadSpriteOrCreate("Assets/Art/Ruby/Characters/Enemy.png", "Enemy", new Color(0.88f, 0.28f, 0.22f, 1f), new Color(0.14f, 0.13f, 0.16f, 1f), SpritePattern.Machine),
            Npc = LoadSpriteOrCreate("Assets/Art/Ruby/Characters/NonPlayerCharacterSheet.png", "EngineerNpc", new Color(0.95f, 0.74f, 0.22f, 1f), new Color(0.18f, 0.22f, 0.24f, 1f), SpritePattern.Character),
            Projectile = LoadSpriteOrCreate("Assets/Art/Ruby/VFX/Projectile.png", "RepairProjectile", new Color(0.42f, 0.95f, 0.96f, 1f), Color.white, SpritePattern.Circle),
            Pickup = LoadSpriteOrCreate("Assets/Art/Ruby/VFX/CollectibleHealth.png", "HealthCollectible", new Color(0.30f, 0.85f, 0.38f, 1f), Color.white, SpritePattern.Cross),
            Grass = LoadSpriteOrCreate("Assets/Art/Ruby/Tile1.png", "GroundTile", new Color(0.24f, 0.55f, 0.34f, 1f), new Color(0.30f, 0.64f, 0.40f, 1f), SpritePattern.Tile),
            Wall = LoadSpriteOrCreate("Assets/Art/Ruby/Tile2.png", "WallTile", new Color(0.33f, 0.36f, 0.38f, 1f), new Color(0.54f, 0.58f, 0.60f, 1f), SpritePattern.Tile),
            Hazard = LoadSpriteOrCreate("Assets/Art/Ruby/Environment/DamageZone.png", "HazardZone", new Color(0.76f, 0.20f, 0.14f, 1f), new Color(1.00f, 0.56f, 0.26f, 1f), SpritePattern.Stripes),
            Crate = LoadSpriteOrCreate("Assets/Art/Ruby/Environment/Decoration_1.png", "Crate", new Color(0.58f, 0.39f, 0.20f, 1f), new Color(0.84f, 0.63f, 0.35f, 1f), SpritePattern.Box),
            Flower = LoadSpriteOrCreate("Assets/Art/Ruby/Environment/Decoration_6.png", "Flower", new Color(0.86f, 0.24f, 0.55f, 1f), new Color(1.00f, 0.92f, 0.24f, 1f), SpritePattern.Circle)
        };
    }

    private enum SpritePattern
    {
        Tile,
        Character,
        Machine,
        Circle,
        Cross,
        Stripes,
        Box
    }

    private static Sprite CreateSprite(string name, Color baseColor, Color accentColor, SpritePattern pattern)
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Color pixel = Color.clear;
                float nx = (x + 0.5f) / size;
                float ny = (y + 0.5f) / size;
                Vector2 center = new Vector2(nx - 0.5f, ny - 0.5f);

                switch (pattern)
                {
                    case SpritePattern.Tile:
                        pixel = ((x / 8 + y / 8) % 2 == 0) ? baseColor : accentColor;
                        break;
                    case SpritePattern.Character:
                        if (center.sqrMagnitude < 0.20f)
                        {
                            pixel = baseColor;
                        }
                        if (ny > 0.54f && Mathf.Abs(nx - 0.38f) < 0.035f)
                        {
                            pixel = accentColor;
                        }
                        if (ny > 0.54f && Mathf.Abs(nx - 0.62f) < 0.035f)
                        {
                            pixel = accentColor;
                        }
                        break;
                    case SpritePattern.Machine:
                        if (Mathf.Abs(center.x) < 0.36f && Mathf.Abs(center.y) < 0.36f)
                        {
                            pixel = baseColor;
                        }
                        if ((x + y) % 17 < 7 && Mathf.Abs(center.x) < 0.30f && Mathf.Abs(center.y) < 0.30f)
                        {
                            pixel = accentColor;
                        }
                        break;
                    case SpritePattern.Circle:
                        if (center.sqrMagnitude < 0.15f)
                        {
                            pixel = baseColor;
                        }
                        if (center.sqrMagnitude < 0.05f)
                        {
                            pixel = accentColor;
                        }
                        break;
                    case SpritePattern.Cross:
                        if (center.sqrMagnitude < 0.17f)
                        {
                            pixel = baseColor;
                        }
                        if ((Mathf.Abs(center.x) < 0.08f || Mathf.Abs(center.y) < 0.08f) && center.sqrMagnitude < 0.16f)
                        {
                            pixel = accentColor;
                        }
                        break;
                    case SpritePattern.Stripes:
                        pixel = ((x + y) / 8 % 2 == 0) ? baseColor : accentColor;
                        break;
                    case SpritePattern.Box:
                        if (Mathf.Abs(center.x) < 0.42f && Mathf.Abs(center.y) < 0.42f)
                        {
                            pixel = ((x / 10 + y / 10) % 2 == 0) ? baseColor : accentColor;
                        }
                        break;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();

        string path = $"Assets/Sprites/{name}.png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 64f;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite LoadSpriteOrCreate(string officialPath, string fallbackName, Color baseColor, Color accentColor, SpritePattern pattern)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(officialPath);
        if (sprite != null)
        {
            return sprite;
        }

        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(officialPath))
        {
            if (asset is Sprite nestedSprite)
            {
                return nestedSprite;
            }
        }

        Debug.LogWarning($"[2D Beginner Adventure] Missing official sprite at {officialPath}; using generated fallback {fallbackName}.");
        return CreateSprite(fallbackName, baseColor, accentColor, pattern);
    }

    private static TileSet CreateTiles(SpriteSet sprites)
    {
        Tile grass = ScriptableObject.CreateInstance<Tile>();
        grass.sprite = sprites.Grass;
        grass.colliderType = Tile.ColliderType.None;
        CreateAssetFresh(grass, "Assets/Tiles/GrassTile.asset");

        Tile wall = ScriptableObject.CreateInstance<Tile>();
        wall.sprite = sprites.Wall;
        wall.colliderType = Tile.ColliderType.Sprite;
        CreateAssetFresh(wall, "Assets/Tiles/WallTile.asset");

        return new TileSet { Grass = grass, Wall = wall };
    }

    private static ParticleSet CreateParticles()
    {
        return new ParticleSet
        {
            Repair = CreateParticlePrefab("RepairParticles", new Color(0.40f, 0.95f, 0.92f, 1f), 24),
            Pickup = CreateParticlePrefab("PickupParticles", new Color(0.52f, 1.00f, 0.48f, 1f), 18)
        };
    }

    private static ParticleSystem CreateParticlePrefab(string name, Color color, short burstCount)
    {
        GameObject root = new GameObject(name);
        ParticleSystem particles = root.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.45f;
        main.startLifetime = 0.42f;
        main.startSpeed = 2.2f;
        main.startSize = 0.12f;
        main.startColor = color;
        main.loop = false;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, burstCount) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.28f;

        ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 20;

        root.AddComponent<ParticleSelfDestruct>();

        string path = $"Assets/Prefabs/{name}.prefab";
        AssetDatabase.DeleteAsset(path);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab.GetComponent<ParticleSystem>();
    }

    private static AnimatorController CreatePlayerAnimator()
    {
        AnimationClip idle = CreateScaleClip("PlayerIdle", 1f, 1.00f, 1.04f, true);
        AnimationClip walk = CreateScaleClip("PlayerWalk", 0.35f, 0.96f, 1.08f, true);
        AnimationClip launch = CreateScaleClip("PlayerLaunch", 0.16f, 1.18f, 1.00f, false);
        AnimationClip hit = CreateColorClip("PlayerHit", 0.20f, new Color(1f, 0.35f, 0.35f, 1f), Color.white, false);

        string path = "Assets/Animations/Player.controller";
        AssetDatabase.DeleteAsset(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("LookX", AnimatorControllerParameterType.Float);
        controller.AddParameter("LookY", AnimatorControllerParameterType.Float);
        controller.AddParameter("Launch", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idleState = machine.AddState("Idle");
        idleState.motion = idle;
        machine.defaultState = idleState;

        AnimatorState walkState = machine.AddState("Walk");
        walkState.motion = walk;
        AddTransition(idleState, walkState, AnimatorConditionMode.Greater, 0.1f, "Speed", false);
        AddTransition(walkState, idleState, AnimatorConditionMode.Less, 0.1f, "Speed", false);

        AnimatorState launchState = machine.AddState("Launch");
        launchState.motion = launch;
        AddAnyTransition(machine, launchState, AnimatorConditionMode.If, 0f, "Launch");
        AddExitTransition(launchState, idleState);

        AnimatorState hitState = machine.AddState("Hit");
        hitState.motion = hit;
        AddAnyTransition(machine, hitState, AnimatorConditionMode.If, 0f, "Hit");
        AddExitTransition(hitState, idleState);

        return controller;
    }

    private static AnimatorController CreateEnemyAnimator()
    {
        AnimationClip moving = CreateScaleClip("EnemyMove", 0.40f, 0.94f, 1.08f, true);
        AnimationClip fixedClip = CreateColorClip("EnemyFixed", 0.35f, new Color(0.62f, 0.92f, 0.82f, 1f), new Color(0.62f, 0.92f, 0.82f, 1f), true);

        string path = "Assets/Animations/Enemy.controller";
        AssetDatabase.DeleteAsset(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Fixed", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState movingState = machine.AddState("Move");
        movingState.motion = moving;
        machine.defaultState = movingState;

        AnimatorState fixedState = machine.AddState("Fixed");
        fixedState.motion = fixedClip;
        AddTransition(movingState, fixedState, AnimatorConditionMode.If, 0f, "Fixed", false);
        return controller;
    }

    private static AnimationClip CreateScaleClip(string name, float duration, float startScale, float peakScale, bool loop)
    {
        AnimationClip clip = new AnimationClip { frameRate = 12f };
        AnimationCurve scaleCurve = new AnimationCurve(
            new Keyframe(0f, startScale),
            new Keyframe(duration * 0.5f, peakScale),
            new Keyframe(duration, startScale));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x"), scaleCurve);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y"), scaleCurve);
        SetLoopTime(clip, loop);
        CreateAssetFresh(clip, $"Assets/Animations/{name}.anim");
        return clip;
    }

    private static AnimationClip CreateColorClip(string name, float duration, Color start, Color end, bool loop)
    {
        AnimationClip clip = new AnimationClip { frameRate = 12f };
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), "m_Color.r"), new AnimationCurve(new Keyframe(0f, start.r), new Keyframe(duration, end.r)));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), "m_Color.g"), new AnimationCurve(new Keyframe(0f, start.g), new Keyframe(duration, end.g)));
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), "m_Color.b"), new AnimationCurve(new Keyframe(0f, start.b), new Keyframe(duration, end.b)));
        SetLoopTime(clip, loop);
        CreateAssetFresh(clip, $"Assets/Animations/{name}.anim");
        return clip;
    }

    private static void SetLoopTime(AnimationClip clip, bool loop)
    {
        SerializedObject serializedObject = new SerializedObject(clip);
        SerializedProperty loopProperty = serializedObject.FindProperty("m_AnimationClipSettings.m_LoopTime");
        if (loopProperty != null)
        {
            loopProperty.boolValue = loop;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AddTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold, string parameter, bool hasExitTime)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = hasExitTime;
        transition.exitTime = hasExitTime ? 0.85f : 0f;
        transition.duration = 0.04f;
        transition.AddCondition(mode, threshold, parameter);
    }

    private static void AddAnyTransition(AnimatorStateMachine machine, AnimatorState to, AnimatorConditionMode mode, float threshold, string parameter)
    {
        AnimatorStateTransition transition = machine.AddAnyStateTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.02f;
        transition.AddCondition(mode, threshold, parameter);
    }

    private static void AddExitTransition(AnimatorState from, AnimatorState to)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = 0.9f;
        transition.duration = 0.04f;
    }

    private static GameObject CreateProjectilePrefab(Sprite sprite, ParticleSystem hitEffect)
    {
        GameObject root = new GameObject("RepairProjectile");
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 5;

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.18f;

        RepairProjectile projectile = root.AddComponent<RepairProjectile>();
        SetObject(projectile, "hitEffectPrefab", hitEffect);

        return SavePrefab(root, "Assets/Prefabs/RepairProjectile.prefab");
    }

    private static GameObject CreatePickupPrefab(Sprite sprite, ParticleSystem pickupEffect)
    {
        GameObject root = new GameObject("HealthCollectible");
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 3;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.28f;

        HealthCollectible pickup = root.AddComponent<HealthCollectible>();
        SetObject(pickup, "pickupEffectPrefab", pickupEffect);

        return SavePrefab(root, "Assets/Prefabs/HealthCollectible.prefab");
    }

    private static GameObject CreateEnemyPrefab(Sprite sprite, AnimatorController animatorController, ParticleSystem repairEffect)
    {
        GameObject root = new GameObject("MalfunctioningEnemy");
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 3;

        Animator animator = root.AddComponent<Animator>();
        animator.runtimeAnimatorController = animatorController;

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.42f;

        root.AddComponent<DamageZone>();
        MalfunctioningEnemy enemy = root.AddComponent<MalfunctioningEnemy>();
        SetObject(enemy, "repairEffectPrefab", repairEffect);

        return SavePrefab(root, "Assets/Prefabs/MalfunctioningEnemy.prefab");
    }

    private static GameObject SavePrefab(GameObject root, string path)
    {
        AssetDatabase.DeleteAsset(path);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private static void CreateWorld(TileSet tiles, SpriteSet sprites)
    {
        GameObject gridObject = new GameObject("Grid");
        gridObject.AddComponent<Grid>();

        Tilemap floor = CreateTilemapLayer(gridObject.transform, "Ground Tilemap", 0, false);
        Tilemap collision = CreateTilemapLayer(gridObject.transform, "Collision Tilemap", 1, true);

        for (int x = -10; x <= 10; x++)
        {
            for (int y = -6; y <= 6; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                floor.SetTile(position, tiles.Grass);

                bool border = x == -10 || x == 10 || y == -6 || y == 6;
                bool block = (x == -3 && y > -5 && y < 2) || (x == 4 && y > -2 && y < 5) || (y == 2 && x > -8 && x < -4);
                if (border || block)
                {
                    collision.SetTile(position, tiles.Wall);
                }
            }
        }

        CreateDecorativeObject("SupplyCrate A", sprites.Crate, new Vector2(-7.2f, -3.5f), true);
        CreateDecorativeObject("SupplyCrate B", sprites.Crate, new Vector2(6.8f, 3.6f), true);
        CreateDecorativeObject("Flower Patch A", sprites.Flower, new Vector2(-5.8f, 4.4f), false);
        CreateDecorativeObject("Flower Patch B", sprites.Flower, new Vector2(2.4f, -4.5f), false);
        CreateHazard("Steam Leak A", sprites.Hazard, new Vector2(-1.2f, -3.6f));
        CreateHazard("Steam Leak B", sprites.Hazard, new Vector2(7.2f, -0.8f));
    }

    private static Tilemap CreateTilemapLayer(Transform parent, string name, int sortingOrder, bool addCollision)
    {
        GameObject layer = new GameObject(name);
        layer.transform.SetParent(parent);
        Tilemap tilemap = layer.AddComponent<Tilemap>();
        TilemapRenderer renderer = layer.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;

        if (addCollision)
        {
            Rigidbody2D body = layer.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;

            TilemapCollider2D tilemapCollider = layer.AddComponent<TilemapCollider2D>();
            tilemapCollider.usedByComposite = true;

            CompositeCollider2D composite = layer.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        }

        return tilemap;
    }

    private static void CreateDecorativeObject(string name, Sprite sprite, Vector2 position, bool solid)
    {
        GameObject item = new GameObject(name);
        item.transform.position = position;
        SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 2;

        if (solid)
        {
            BoxCollider2D collider = item.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.75f, 0.75f);
        }
    }

    private static void CreateHazard(string name, Sprite sprite, Vector2 position)
    {
        GameObject hazard = new GameObject(name);
        hazard.transform.position = position;
        hazard.transform.localScale = new Vector3(1.6f, 1.0f, 1f);

        SpriteRenderer renderer = hazard.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 2;

        BoxCollider2D collider = hazard.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.85f, 0.55f);

        hazard.AddComponent<DamageZone>();
    }

    private static GameObject CreatePlayer(Sprite sprite, AnimatorController animatorController, GameObject projectilePrefab)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(-8f, -4f, 0f);

        SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 4;

        Animator animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = animatorController;

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
        collider.radius = 0.38f;

        Health health = player.AddComponent<Health>();
        PlayerAnimationController animationController = player.AddComponent<PlayerAnimationController>();
        PlayerController2D controller = player.AddComponent<PlayerController2D>();

        GameObject spawn = new GameObject("ProjectileSpawn");
        spawn.transform.SetParent(player.transform);
        spawn.transform.localPosition = new Vector3(0f, -0.55f, 0f);

        SetObject(controller, "projectilePrefab", projectilePrefab.GetComponent<RepairProjectile>());
        SetObject(controller, "projectileSpawn", spawn.transform);
        SetObject(controller, "animationController", animationController);

        return player;
    }

    private static MalfunctioningEnemy[] CreateEnemies(GameObject enemyPrefab)
    {
        MalfunctioningEnemy enemyA = InstantiatePrefab(enemyPrefab, "Enemy Horizontal", new Vector2(-0.4f, 3.8f));
        SetVector2(enemyA, "patrolAxis", Vector2.right);
        SetFloat(enemyA, "patrolDistance", 2.4f);
        SetFloat(enemyA, "speed", 1.6f);

        MalfunctioningEnemy enemyB = InstantiatePrefab(enemyPrefab, "Enemy Vertical", new Vector2(6.2f, -3.4f));
        SetVector2(enemyB, "patrolAxis", Vector2.up);
        SetFloat(enemyB, "patrolDistance", 2.2f);
        SetFloat(enemyB, "speed", 1.3f);

        MalfunctioningEnemy enemyC = InstantiatePrefab(enemyPrefab, "Enemy Gate", new Vector2(7.2f, 4.3f));
        SetVector2(enemyC, "patrolAxis", Vector2.left);
        SetFloat(enemyC, "patrolDistance", 1.8f);
        SetFloat(enemyC, "speed", 1.1f);

        return new[] { enemyA, enemyB, enemyC };
    }

    private static MalfunctioningEnemy InstantiatePrefab(GameObject prefab, string name, Vector2 position)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = name;
        instance.transform.position = position;
        return instance.GetComponent<MalfunctioningEnemy>();
    }

    private static void CreatePickups(GameObject pickupPrefab)
    {
        Vector2[] positions =
        {
            new Vector2(-6.8f, 2.8f),
            new Vector2(1.5f, -2.7f),
            new Vector2(8.2f, 2.0f)
        };

        foreach (Vector2 position in positions)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(pickupPrefab);
            instance.name = "HealthCollectible";
            instance.transform.position = position;
        }
    }

    private static DialogueActor CreateNpc(Sprite sprite)
    {
        GameObject npc = new GameObject("Engineer NPC");
        npc.transform.position = new Vector3(8.2f, 4.7f, 0f);

        SpriteRenderer renderer = npc.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 3;

        CircleCollider2D collider = npc.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.45f;

        DialogueActor dialogue = npc.AddComponent<DialogueActor>();
        SetString(dialogue, "message", "Nice work. The machines are stable again, and the workshop is safe.");
        return dialogue;
    }

    private static void CreateHud(Health playerHealth)
    {
        PanelSettings panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.name = "HudPanelSettings";
        panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panelSettings.referenceResolution = new Vector2Int(1280, 720);
        CreateAssetFresh(panelSettings, "Assets/UI/HudPanelSettings.asset");

        GameObject hud = new GameObject("HUD");
        UIDocument document = hud.AddComponent<UIDocument>();
        document.panelSettings = panelSettings;
        document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Hud.uxml");

        HudDocumentController controller = hud.AddComponent<HudDocumentController>();
        SetObject(controller, "playerHealth", playerHealth);
    }

    private static void CreateCamera(Transform target)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(target.position.x, target.position.y, -10f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.backgroundColor = new Color(0.08f, 0.13f, 0.15f, 1f);

        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<AudioSource>();
        cameraObject.AddComponent<BackgroundToneLoop>();

        CameraFollow2D follow = cameraObject.AddComponent<CameraFollow2D>();
        SetObject(follow, "target", target);
    }

    private static void CreateObjectiveTracker(MalfunctioningEnemy[] enemies, DialogueActor npc)
    {
        GameObject tracker = new GameObject("Objective Tracker");
        RepairObjectiveTracker objectiveTracker = tracker.AddComponent<RepairObjectiveTracker>();

        SerializedObject serializedObject = new SerializedObject(objectiveTracker);
        SerializedProperty enemiesProperty = serializedObject.FindProperty("enemies");
        enemiesProperty.arraySize = enemies.Length;
        for (int i = 0; i < enemies.Length; i++)
        {
            enemiesProperty.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i];
        }

        serializedObject.FindProperty("completionNpc").objectReferenceValue = npc;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObject(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(propertyName).objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(propertyName).floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetVector2(UnityEngine.Object target, string propertyName, Vector2 value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(propertyName).vector2Value = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(UnityEngine.Object target, string propertyName, string value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(propertyName).stringValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateAssetFresh(UnityEngine.Object asset, string path)
    {
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.ImportAsset(path);
    }

    private static void RequireAsset<T>(string path) where T : UnityEngine.Object
    {
        if (AssetDatabase.LoadAssetAtPath<T>(path) == null)
        {
            throw new FileNotFoundException($"Missing required asset: {path}");
        }
    }
}
