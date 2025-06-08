using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Proje0001.Models
{
    public class Word
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        [BsonElement("text")]
        public string Text { get; set; }

        [Required]
        [StringLength(100)]
        [BsonElement("translation")]
        public string Translation { get; set; }

        [StringLength(500)]
        [BsonElement("description")]
        public string Description { get; set; }

        [Required]
        [BsonElement("category")]
        public string Category { get; set; }

        [Required]
        [BsonElement("difficultyLevel")]
        public int DifficultyLevel { get; set; }

        [BsonElement("examples")]
        public List<string> Examples { get; set; } = new List<string>();

        [BsonElement("synonyms")]
        public List<string> Synonyms { get; set; } = new List<string>();

        [BsonElement("antonyms")]
        public List<string> Antonyms { get; set; } = new List<string>();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        public void Update(Word updatedWord)
        {
            if (updatedWord == null)
                throw new ArgumentNullException(nameof(updatedWord));

            Text = updatedWord.Text;
            Translation = updatedWord.Translation;
            Description = updatedWord.Description;
            Category = updatedWord.Category;
            DifficultyLevel = updatedWord.DifficultyLevel;
            Examples = updatedWord.Examples;
            Synonyms = updatedWord.Synonyms;
            Antonyms = updatedWord.Antonyms;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
                return false;

            if (string.IsNullOrWhiteSpace(Translation))
                return false;

            if (string.IsNullOrWhiteSpace(Category))
                return false;

            if (DifficultyLevel < 1 || DifficultyLevel > 5)
                return false;

            return true;
        }
    }
} 