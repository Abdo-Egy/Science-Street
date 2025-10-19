using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AL_Arcade.DialogueSystem.Scripts
{
    /// <summary>
    /// Singleton that builds and maintains comprehensive game context for AI.
    /// Persists across scenes.
    /// </summary>
    public class GameContextBuilder : MonoBehaviour
    {
        #region Singleton
        private static GameContextBuilder instance;
        public static GameContextBuilder Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("GameContextBuilder");
                    instance = go.AddComponent<GameContextBuilder>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        #endregion

        #region Context Data Structure
        [Serializable]
        private class GameContext
        {
            public string gameName = "";
            public string gameDescription = "";
            public string coreMechanics = "";
            public List<string> playerActions = new List<string>();
            public string currentObjective = "";
        }
        #endregion

        #region Private Fields
        private GameContext context = new GameContext();
        private readonly StringBuilder contextBuilder = new StringBuilder();
        private const int MAX_ACTIONS = 100; // Limit to prevent excessive context size
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[GameContextBuilder] Singleton initialized and persisting across scenes");
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Public API - Initialization
        /// <summary>
        /// Initializes static game information. Call once at game start.
        /// </summary>
        /// <param name="name">Name of the game</param>
        /// <param name="description">Brief description of the game</param>
        /// <param name="mechanics">Core game mechanics explanation</param>
        public void InitializeGame(string name, string description, string mechanics)
        {
            context.gameName = name ?? "";
            context.gameDescription = description ?? "";
            context.coreMechanics = mechanics ?? "";

            Debug.Log($"[GameContextBuilder] Game initialized: {name}");
            Debug.Log($"[GameContextBuilder] Description: {description}");
            Debug.Log($"[GameContextBuilder] Mechanics: {mechanics}");
        }
        #endregion

        #region Public API - Player Actions
        /// <summary>
        /// Adds a player action to the chronological history.
        /// </summary>
        /// <param name="action">Description of the player action</param>
        public void AddPlayerAction(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                Debug.LogWarning("[GameContextBuilder] Attempted to add empty action");
                return;
            }

            // Add timestamp for better context
            string timestampedAction = $"[{DateTime.Now:HH:mm:ss}] {action}";
            context.playerActions.Add(timestampedAction);

            // Limit actions list to prevent excessive size
            if (context.playerActions.Count > MAX_ACTIONS)
            {
                int removeCount = context.playerActions.Count - MAX_ACTIONS;
                context.playerActions.RemoveRange(0, removeCount);
                Debug.Log($"[GameContextBuilder] Trimmed {removeCount} old actions (exceeded MAX_ACTIONS)");
            }

            Debug.Log($"[GameContextBuilder] Action added: {action} (Total: {context.playerActions.Count})");
        }

        /// <summary>
        /// Adds multiple player actions at once.
        /// </summary>
        public void AddPlayerActions(params string[] actions)
        {
            foreach (string action in actions)
            {
                AddPlayerAction(action);
            }
        }

        /// <summary>
        /// Clears all player actions. Useful for level transitions.
        /// </summary>
        public void ClearActions()
        {
            int count = context.playerActions.Count;
            context.playerActions.Clear();
            Debug.Log($"[GameContextBuilder] Cleared {count} player actions");
        }
        #endregion

        #region Public API - Current Objective
        /// <summary>
        /// Sets or updates the current player objective.
        /// </summary>
        /// <param name="objective">Current objective description</param>
        public void SetCurrentObjective(string objective)
        {
            context.currentObjective = objective ?? "";
            Debug.Log($"[GameContextBuilder] Objective set: {objective}");
        }

        /// <summary>
        /// Clears the current objective.
        /// </summary>
        public void ClearObjective()
        {
            context.currentObjective = "";
            Debug.Log("[GameContextBuilder] Objective cleared");
        }
        #endregion

        #region Public API - Context Retrieval
        /// <summary>
        /// Gets the complete formatted game context as a single string.
        /// Returns FULL, UNTRIMMED context for AI processing.
        /// </summary>
        /// <returns>Formatted context string</returns>
        public string GetFullContext()
        {
            contextBuilder.Clear();

            // Game Information Section
            contextBuilder.AppendLine("=== GAME INFORMATION ===");
            
            if (!string.IsNullOrEmpty(context.gameName))
            {
                contextBuilder.AppendLine($"GAME: {context.gameName}");
            }

            if (!string.IsNullOrEmpty(context.gameDescription))
            {
                contextBuilder.AppendLine($"DESCRIPTION: {context.gameDescription}");
            }

            if (!string.IsNullOrEmpty(context.coreMechanics))
            {
                contextBuilder.AppendLine($"MECHANICS: {context.coreMechanics}");
            }

            contextBuilder.AppendLine();

            // Player Actions Section
            contextBuilder.AppendLine("=== PLAYER ACTIONS (CHRONOLOGICAL) ===");
            
            if (context.playerActions.Count > 0)
            {
                for (int i = 0; i < context.playerActions.Count; i++)
                {
                    contextBuilder.AppendLine($"{i + 1}. {context.playerActions[i]}");
                }
            }
            else
            {
                contextBuilder.AppendLine("(No actions recorded yet)");
            }

            contextBuilder.AppendLine();

            // Current Objective Section
            contextBuilder.AppendLine("=== CURRENT OBJECTIVE ===");
            
            if (!string.IsNullOrEmpty(context.currentObjective))
            {
                contextBuilder.AppendLine(context.currentObjective);
            }
            else
            {
                contextBuilder.AppendLine("(No objective set)");
            }

            string fullContext = contextBuilder.ToString();
            
            Debug.Log($"[GameContextBuilder] Generated context ({fullContext.Length} characters)");
            
            return fullContext;
        }

        /// <summary>
        /// Gets a summary of the current context without full details.
        /// </summary>
        public string GetContextSummary()
        {
            return $"Game: {context.gameName} | Actions: {context.playerActions.Count} | " +
                   $"Objective: {(string.IsNullOrEmpty(context.currentObjective) ? "None" : "Set")}";
        }
        #endregion

        #region Public API - Context Management
        /// <summary>
        /// Completely resets all context data.
        /// </summary>
        public void ResetAllContext()
        {
            context = new GameContext();
            Debug.Log("[GameContextBuilder] All context reset");
        }

        /// <summary>
        /// Gets the current number of recorded actions.
        /// </summary>
        public int GetActionCount()
        {
            return context.playerActions.Count;
        }

        /// <summary>
        /// Checks if game has been initialized.
        /// </summary>
        public bool IsGameInitialized()
        {
            return !string.IsNullOrEmpty(context.gameName);
        }
        #endregion

        #region Debug Helpers
        /// <summary>
        /// Logs the full context to console. Useful for debugging.
        /// </summary>
        [ContextMenu("Log Full Context")]
        public void LogFullContext()
        {
            string fullContext = GetFullContext();
            Debug.Log($"[GameContextBuilder] FULL CONTEXT:\n{fullContext}");
        }

        /// <summary>
        /// Logs a summary of the current context.
        /// </summary>
        [ContextMenu("Log Context Summary")]
        public void LogContextSummary()
        {
            Debug.Log($"[GameContextBuilder] SUMMARY: {GetContextSummary()}");
        }
        #endregion
    }
}