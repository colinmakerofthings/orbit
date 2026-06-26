"""
Automation entrypoint for STARS Enterprise and solutions.
"""
from orbit.core.config import load_config
from orbit.core.stars import run_stars_enterprise, run_sdm, run_vtrs, run_data_client, run_demo_data
from orbit.core.logging import log

def automate_all(config_path: str = "orbit/config/orbit.yaml", dry_run: bool = False):
    cfg = load_config(config_path)
    repos = cfg['repos']
    solutions = cfg['solutions']
    if solutions['stars_enterprise']['enabled']:
        run_stars_enterprise(solutions['stars_enterprise'], repos, dry_run=dry_run)
    if solutions['sdm']['enabled']:
        run_sdm(solutions['sdm'], repos, dry_run=dry_run)
    if solutions['vtrs']['enabled']:
        run_vtrs(solutions['vtrs'], repos, dry_run=dry_run)
    # data_client and demo_data removed per user request
    log("Automation complete.", color="green")
