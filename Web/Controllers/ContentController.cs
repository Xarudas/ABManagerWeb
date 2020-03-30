﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ABManagerWeb.ApplicationCore.Entities;
using ABManagerWeb.ApplicationCore.Helpers.Paths;
using ABManagerWeb.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ABManagerWeb.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IManifestManager _manager;
        private readonly ILogger<ContentController> _logger;
        public ContentController(IManifestManager manager, ILogger<ContentController> logger)
        {
            _manager = manager;
            _logger = logger;
        }

        public string AssetBundleHosting { get; private set; }

        [HttpGet("{version}/manifest")]
        public async Task<IActionResult> DownloadManifest(string version)
        {
            _logger.LogInformation($"GetManifest");
            var manifestInfo = await _manager.GetManifestByVersionAsync(version);
            if (manifestInfo != null)
            {
                return GetDownloadedManifestFile(manifestInfo);
            }
            return BadRequest();
        }
        [HttpGet("{version}/manifest/info")]
        public async Task<IActionResult> GetManifestInfo(string version)
        {
            _logger.LogInformation($"GetManifestInfo");
            var manifestInfo = await _manager.GetManifestByVersionAsync(version);
            if (manifestInfo != null)
            {
                return new JsonResult(manifestInfo);
            }
            return BadRequest();
        }
        [HttpGet("manifest")]
        public async Task<IActionResult> DownloadCurrentManifest()
        {
            _logger.LogInformation($"GetManifest");
            var manifestInfo = await _manager.GetCurrentManifestAsync();
            if (manifestInfo != null)
            {
                return GetDownloadedManifestFile(manifestInfo);
            }
            return BadRequest();
        }
        [HttpGet("manifest/info")]
        public async Task<IActionResult> GetCurrentManifestInfo()
        {
            _logger.LogInformation($"GetManifest");
            var manifestInfo = await _manager.GetCurrentManifestAsync();
            if (manifestInfo != null)
            {
                return new JsonResult(manifestInfo);
            }
            return BadRequest();
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadManifest(IFormFile manifestFile)
        {
            _logger.LogDebug("UploadManifest");
            if (manifestFile != null)
            {
                _logger.LogDebug("Manifest file is not null");
                string version = await _manager.GetManifestVersionByStreamAsync(() => manifestFile.OpenReadStream());
                await _manager.AddManifestAsync(version, (fileStream) => manifestFile.CopyToAsync(fileStream));
                var currentManifest = await _manager.GetCurrentManifestAsync();
                if (currentManifest != null)
                {
                    return Ok();
                }
            }
            return BadRequest();
        }

        private FileStreamResult GetDownloadedManifestFile(ManifestInfo manifestInfo)
        {
            string path = Path.Combine(ABHostingPaths.GetMainPath(), manifestInfo.Path);
            var manifestFile = new FileStream(path, FileMode.Open);
            var contentType = "application/json";
            var fileName = "manifest.json";
            return File(manifestFile, contentType, fileName);
        }
    }
}