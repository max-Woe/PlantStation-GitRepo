
import sys
import json
from pathlib import Path
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker, declarative_base

def resource_path(relative_path):
    if hasattr(sys, '_MEIPASS'):
        return Path(sys._MEIPASS) / relative_path
    return (Path(__file__).resolve().parent / "../../" / relative_path).resolve()


Base = declarative_base()

secrets_path = resource_path("secrets.json")

try:
    with open(secrets_path, 'r') as f:
        secrets = json.load(f)
except FileNotFoundError:
    print(f"Fehler: Die Datei {secrets_path} wurde nicht gefunden.")
    sys.exit(1)

DATABASE_URL = f"postgresql+psycopg2://{secrets['DB_USER']}:{secrets['DB_PASSWORD']}@{secrets['DB_HOST']}:{secrets['DB_PORT']}/{secrets['DB_NAME']}"
engine = create_engine(DATABASE_URL, echo=True)

SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)