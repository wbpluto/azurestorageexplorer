﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prometheus;
using StorageLibrary.Common;

namespace AzureWebStorageExplorer.Controllers
{
    [Produces("application/json")]
    [Route("api/Files")]
    public class FilesController : CounterController
    {
        private static readonly Counter FilesCounter = Metrics.CreateCounter("filescontroller_counter_total", "Keep FilesController access count");

        [HttpGet("[action]")]
		public async Task<IEnumerable<string>> GetShares(string account, string key)
		{
            Increment(FilesCounter);

			IEnumerable<FileShareWrapper> shares = await StorageLibrary.File.ListFileSharesAsync(account, key);

			return shares.Select(s => s.Name);
		}

        [HttpGet("[action]")]
        public async Task<IEnumerable<FileShareItemWrapper>> GetFilesAndDirectories(string account, string key, string share, string folder)
        {
            Increment(FilesCounter);

            return await StorageLibrary.File.ListFilesAndDirsAsync(account, key, share, folder);
        }

        [HttpGet("[action]")]
        public async Task<FileResult> GetFile(string account, string key, string share, string file, string folder= null)
        {
            Increment(FilesCounter);

            if (string.IsNullOrEmpty(share))
                return null;

            string filePath = await StorageLibrary.File.GetFile(account, key, share, file, folder);

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            Response.Headers.Add("Content-Disposition", $"inline; filename={HttpUtility.UrlEncode(file)}");

            return File(fileBytes, "application/octet-stream", file);

        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteFile(string account, string key, string share, string file, string folder = null)
        {
            Increment(FilesCounter);

            if (string.IsNullOrEmpty(file))
                return BadRequest();

            await StorageLibrary.File.DeleteFileAsync(account, key, share, file,folder);

            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFile(string account, string key, string share, string folder, string fileName, List<IFormFile> files)
        {
            Increment(FilesCounter);

            foreach (IFormFile file in files)
                await StorageLibrary.File.CreateFileAsync(account, key, share, fileName, file.OpenReadStream(), folder);

            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateSubDir(string account, string key, string share, string subDir, string folder = null)
        {
            Increment(FilesCounter);

            await StorageLibrary.File.CreateSubDirectory(account, key, share, subDir, folder);

            return Ok();
        }
    }
}