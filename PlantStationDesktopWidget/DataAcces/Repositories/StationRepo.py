import pandas as pd
from typing import List, cast
from DataAcces.Models.Base import SessionLocal
from DataAcces.Models.Station import Station

class StationRepo:
    def __init__(self, session_factory=SessionLocal):
        self.session_factory = session_factory


    def get_all(self) -> pd.DataFrame:
        with self.session_factory() as session:
            query = session.query(Station)
            df = pd.read_sql(query.statement, session.bind)
            df = cast(pd.DataFrame, df)

            if df.empty:
                return pd.DataFrame()

            return df

    def get_all_station_ids(self) -> List[int]:
        with self.session_factory() as session:
            stations = session.query(Station).all()
            return [station.id for station in stations]
