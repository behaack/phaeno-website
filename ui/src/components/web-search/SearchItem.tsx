import type { articletypes, webtypes } from '@/assets/docTypes';
import { useMemo } from 'react';
import { BsFiletypeHtml } from "react-icons/bs";
import SearchHighlightedSnippet from './SearchHighlightedSnippet';

export interface ISearchItem {
  id: string;
  url: string;
  pageTitle: string;
  anchor: string;
  anchorTitle: string;
  description: string;
  documentType: webtypes | articletypes;
  snippet: string;
  count: number;
  score: number;
}

export interface IProps {
  list: ISearchItem[];
  index: number;
  item: ISearchItem;
  searchStr: string;
  active: boolean;
  linkRef: (node: HTMLAnchorElement | null) => void;
  optionId: string;
  onSelect: () => void;
  onFocusOption: (index: number) => void;
}

export default function SearchItem({ 
  list, 
  index, 
  item, 
  searchStr, 
  active, 
  linkRef, 
  optionId, 
  onSelect,
  onFocusOption
}: IProps ) {
  const isHeader = useMemo(() => {
    if (index === 0) return true;
    return list[index - 1].pageTitle !== list[index].pageTitle;
  }, [index, list]);

  const header = useMemo(() => (
    <li role="presentation" aria-hidden="true" className="bg-[#789946] text-white p-2">
      <div className="flex gap-2 items-center">
        <BsFiletypeHtml size={16} />
        <h3 className="p-0 m-0 text-sm text-white font-semibold inline-flex items-center gap-2">
          {item.pageTitle}
        </h3>
      </div>
    </li>
  ), [item.pageTitle]);

  const link = useMemo(() => (
    <li
      role="presentation"
      className={`web-search-item ${active ? 'web-search-item-active' : ''}`}
    >
      <a
        href={item.url}
        ref={linkRef}
        className="web-search-link"
        role="option"
        aria-selected={active}
        id={optionId}
        tabIndex={active ? 0 : -1}
        onFocus={() => onFocusOption(index)}
        onClick={(e) => { 
          e.preventDefault();
          onSelect();
          window.location.href = item.url;
        }}
      >
        <div className="p-1">
          <div className="flex items-center gap-1">
            <div className="p-0 m-0 text-sm font-semibold inline-flex items-center">
              <div>{item.anchorTitle}</div>
            </div>
            <div className="text-[9px] bg-amber-800 text-white max-w-fit py-1 px-2 rounded-lg font-bold">
              {item.count}&nbsp;{(item.count===1)?'match':'matches'}
            </div>
          </div>
          <div className="text-[10px]">
            <SearchHighlightedSnippet text={item.snippet} searchStr={searchStr} />
          </div>
        </div>
      </a>
    </li>
  ), [item, searchStr, active, linkRef, optionId, onFocusOption, onSelect]);

  return (
    <>
      {isHeader && header}
      {link}
    </>
  );
}
