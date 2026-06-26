"""
Config loader and validator for Orbit YAML config.
"""
import yaml
from typing import Any, Dict
from orbit.core.logging import log

def load_config(path: str) -> Dict[str, Any]:
    try:
        with open(path, 'r') as f:
            config = yaml.safe_load(f)
        # TODO: Add schema validation here
        return config
    except Exception as e:
        log(f"Failed to load config: {e}", level="error")
        raise
