"""
STARS Enterprise automation logic for Orbit.
"""
import os
import glob
from typing import Dict, Any
from orbit.core.execution import run_command
from orbit.core.logging import log
import time

def wait_for_pods(timeout_min: int = 10):
    log("Waiting for all pods to be Running...", color="cyan")
    start = time.time()
    while True:
        result = run_command(["kubectl", "get", "pods", "-n", "default", "--no-headers"])
        pods = result.output.strip().splitlines()
        not_ready = [p for p in pods if not any(s in p for s in [" Running ", " Completed "])]
        if not not_ready and pods:
            break
        if (time.time() - start) > timeout_min * 60:
            log("Timeout waiting for pods to become ready.", level="warn", color="yellow")
            break
        time.sleep(5)
    log("All pods running.", color="green")

def auto_discover_file(directory: str, pattern: str) -> str:
    files = glob.glob(os.path.join(directory, pattern))
    if not files:
        raise FileNotFoundError(f"No files matching {pattern} in {directory}")
    return files[0]

def run_stars_enterprise(cfg: Dict[str, Any], repos: Dict[str, str], dry_run: bool = False):
    se_repo = repos['stars_enterprise']
    script = os.path.join(se_repo, cfg['deploy_script'])
    run_command(["bash", script], dry_run=dry_run)
    if cfg.get('wait_for_pods', True):
        wait_for_pods()

def run_sdm(cfg: Dict[str, Any], repos: Dict[str, str], dry_run: bool = False):
    if not cfg.get('enabled', True):
        return
    dir_path = cfg['package_dir'].format(**repos)
    pkg = auto_discover_file(dir_path, cfg['package_glob'])
    cmd = cfg['install_command'].format(package_path=pkg)
    run_command(cmd.split(), dry_run=dry_run)

def run_vtrs(cfg: Dict[str, Any], repos: Dict[str, str], dry_run: bool = False):
    vtrs_repo = repos[cfg['repo']]
    script = os.path.join(vtrs_repo, cfg['deploy_script'])
    run_command(["bash", script], dry_run=dry_run)
    if cfg.get('wait_for_pods', True):
        wait_for_pods()
