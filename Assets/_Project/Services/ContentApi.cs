using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using BookLab.Core.Net;
using BookLab.Models;

namespace BookLab.Services
{
    // The "front door" the whole app calls (same role as your GameBullApi).
    // Nobody else deals with URLs or JSON — they just call Save / Load / List / Catalog.
    public static class ContentApi
    {
        // ---- The picker menu (backgrounds + stickers) ----
        public static async Task<AssetCatalog> GetCatalog()
        {
            string json = await FirebaseClient.GetJson(FirebaseEndpoints.Catalog());
            if (IsEmpty(json)) return new AssetCatalog();

            try
            {
                var catalog = JsonConvert.DeserializeObject<AssetCatalog>(json) ?? new AssetCatalog();
                StampIds(catalog.backgrounds);   // in Firebase the id is the KEY; copy it into each object
                StampIds(catalog.objects);
                return catalog;
            }
            catch (Exception e)
            {
                Debug.LogError("[ContentApi] Catalog parse failed: " + e.Message);
                return new AssetCatalog();
            }
        }

        // ---- One kid's saved books (for the shelf) ----
        // NOTE: this pulls the kid's full books. At big scale you'd keep a tiny
        // separate index (title + cover only) so the shelf stays light.
        public static async Task<List<BookModel>> ListBooks(string kidId)
        {
            var result = new List<BookModel>();
            string json = await FirebaseClient.GetJson(FirebaseEndpoints.Books(kidId));
            if (IsEmpty(json)) return result;   // no books yet

            try
            {
                var map = JsonConvert.DeserializeObject<Dictionary<string, BookModel>>(json);
                if (map == null) return result;
                foreach (var kv in map)
                {
                    if (kv.Value == null) continue;
                    kv.Value.id = kv.Key;        // the key is the bookId
                    result.Add(kv.Value);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ContentApi] ListBooks parse failed: " + e.Message);
            }
            return result;
        }

        // ---- Load one full book (to view or edit) ----
        public static async Task<BookModel> LoadBook(string kidId, string bookId)
        {
            string json = await FirebaseClient.GetJson(FirebaseEndpoints.Book(kidId, bookId));
            if (IsEmpty(json)) return null;
            try
            {
                var book = JsonConvert.DeserializeObject<BookModel>(json);
                if (book != null) book.id = bookId;
                return book;
            }
            catch (Exception e)
            {
                Debug.LogError("[ContentApi] LoadBook parse failed: " + e.Message);
                return null;
            }
        }

        // ---- Save (create or overwrite) a book ----
        public static async Task<bool> SaveBook(string kidId, BookModel book)
        {
            if (book == null) return false;

            string now = DateTime.UtcNow.ToString("o");   // ISO 8601 timestamp
            if (string.IsNullOrEmpty(book.id))
            {
                book.id        = "book_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                book.createdAt = now;
            }
            book.updatedAt = now;

            string json = JsonConvert.SerializeObject(book);
            return await FirebaseClient.PutJson(FirebaseEndpoints.Book(kidId, book.id), json);
        }

        // ---- small helpers ----
        static bool IsEmpty(string json) => string.IsNullOrEmpty(json) || json == "null";

        static void StampIds(Dictionary<string, AssetDefinition> map)
        {
            if (map == null) return;
            foreach (var kv in map)
                if (kv.Value != null) kv.Value.id = kv.Key;
        }
    }
}
