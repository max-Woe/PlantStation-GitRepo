import pandas as pd
import logging
import sqlalchemy
from typing import List, cast
from DataAcces.Models.Base import SessionLocal
from DataAcces.Models.Station import Station

logger = logging.getLogger(__name__)

class StationRepo:
    def __init__(self, session_factory=SessionLocal):
        self.session_factory = session_factory


    def get_all(self) -> pd.DataFrame:
        with self.session_factory() as session:
            try:
                query = session.query(Station)
                df = pd.read_sql(query.statement, session.bind)
                df = cast(pd.DataFrame, df)

                return df

            except sqlalchemy.exc.ProgrammingError as ex:
                # Schema-Mismatch, Tabelle existiert nicht
                logger.error(f"DB schema error: {ex}")
                return pd.DataFrame()

            except sqlalchemy.exc.InvalidRequestError as ex:
                # Session-Problem
                logger.error(f"DB session error: {ex}")
                return pd.DataFrame()


    def get_all_station_ids(self) -> List[int]:
        with self.session_factory() as session:
            try:
                stations = session.query(Station).all()
                return [station.id for station in stations]

            except sqlalchemy.exc.ProgrammingError as ex:
                # Schema-Mismatch, Tabelle existiert nicht
                logger.error(f"DB schema error: {ex}")
                return pd.DataFrame()

            except sqlalchemy.exc.InvalidRequestError as ex:
                # Session-Problem
                logger.error(f"DB session error: {ex}")
                return pd.DataFrame()
